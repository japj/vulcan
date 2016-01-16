using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Reflection;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;

namespace Ssis2008Emitter.Emitters.TSQL
{
    public class TemplateManager
    {
        private XmlSchemaSet _templateSchemaSet;
        private Dictionary<string, Template> templateDictionary;

        public TemplateManager()
        {
            
            string localDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string templatePath = String.Format("{0}{1}{2}", localDirectory, Path.DirectorySeparatorChar, Settings.Default.SubpathSsis2008EmitterContent);
            string templateSchemaFilename = String.Format("{0}{1}{2}", templatePath, Path.DirectorySeparatorChar, Settings.Default.SubpathTemplateSchemaFile);
            _templateSchemaSet = new XmlSchemaSet();
            _templateSchemaSet.Add(null, templateSchemaFilename);
            templateDictionary = new Dictionary<string, Template>();

            MessageEngine.Global.Trace(Severity.Debug, "Template Manager: Created with Path {0}", templatePath);
            foreach (string file in Directory.GetFiles(templatePath))
            {
                this.AddTemplateFile(file);
            }
        }

        public void AddTemplate(string templateName, Template template)
        {
            this.templateDictionary.Add(templateName, template);
        }

        public void AddTemplateFile(string fileName)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = _templateSchemaSet;
            settings.ValidationEventHandler += new ValidationEventHandler(settings_ValidationEventHandler);

            XmlReader templateXmlReader = XmlReader.Create(fileName, settings);
            XmlDocument templateDocument = new XmlDocument();
            templateDocument.Load(templateXmlReader);

            MessageEngine.Global.Trace(Severity.Debug, "TemplateManager: Validating {0}", fileName);
            templateDocument.Validate(settings_ValidationEventHandler);
            MessageEngine.Global.Trace(Severity.Debug, "TemplateManager: Validation Succeeded");

            XPathNavigator templateNavigator = templateDocument.CreateNavigator();

            XmlNamespaceManager templateNamespaceManager = new XmlNamespaceManager(templateNavigator.NameTable);
            templateNamespaceManager.AddNamespace("tm", "http://schemas.microsoft.com/detego/2007/07/07/VulcanTemplate.xsd");

            foreach (XPathNavigator nav in templateNavigator.Select("//tm:Templates/tm:Template", templateNamespaceManager))
            {
                string templateName = nav.SelectSingleNode("@Name").Value.Trim();
                string templateData = nav.SelectSingleNode("tm:TemplateData", templateNamespaceManager).Value.Trim();
                string templateType = nav.SelectSingleNode("@Type").Value.Trim();

                Template t = new Template(templateName, templateType, templateData);
                MessageEngine.Global.Trace(Severity.Debug, "Adding template " + templateName);

                if (templateType == "XMLTemplate")
                {
                    MessageEngine.Global.Trace(Severity.Debug, "{0} is an XML Template", templateName);
                }
                foreach (XPathNavigator mapNav in nav.Select("tm:Map", templateNamespaceManager))
                {
                    string source = mapNav.SelectSingleNode("@Source").Value;
                    int index = mapNav.SelectSingleNode("@Index").ValueAsInt;
                    MessageEngine.Global.Trace(Severity.Debug, "Mapping input {0} to index {1}", source, index);
                    t.MapDictionary.Add(source, index);
                }

                templateDictionary.Add(templateName, t);
            }
        }

        void settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            MessageEngine.Global.Trace(Severity.Error, "TemplateManager: Schema Validation Error: " + e.Message);
        }

        /*
        public VulcanConfig XmlTemplateReplacement(VulcanConfig vulcanConfig)
        {
            List<XPathNavigator> nodeList = new List<XPathNavigator>();
            VulcanConfig vc = vulcanConfig;

            foreach (XPathNavigator node in vc.Navigator.SelectDescendants(XPathNodeType.Element, true))
            {
                if (node.XmlType != null && node.XmlType.Name != null)
                {
                    if (node.XmlType.Name.ToUpperInvariant().Contains("TEMPLATEXML"))
                    {
                        nodeList.Add(node);
                    }
                }
            }

            foreach (XPathNavigator node in nodeList)
            {
                XPathNavigator nameNode = node.SelectSingleNode("@Name");
                string templateName = nameNode.Value;
                nameNode.DeleteSelf();
                MessageEngine.Global.Trace(Severity.Debug, "Replacing Template {0}", node.OuterXml);
                if (this.templateDictionary.ContainsKey(templateName))
                {
                    Template t = this[templateName];
                    TemplateEmitter te = new TemplateEmitter(t);

                    int parameterCount = 0;
                    if (node.MoveToFirstAttribute())
                    {
                        do
                        {
                            parameterCount++;
                            MessageEngine.Global.Trace(Severity.Debug, "Mapping Parameter {0}={1}", node.Name, node.Value);
                            te.SetNamedParameter(node.Name, node.Value);
                        } while (node.MoveToNextAttribute());
                    }
                    if (parameterCount == t.MapDictionary.Keys.Count)
                    {
                        string newXml;
                        te.Emit(out newXml);
                        if (parameterCount > 0)
                        {
                            node.MoveToParent();
                        }
                        MessageEngine.Global.Trace(Severity.Debug, "Old Node: {0}", node.OuterXml);
                        node.OuterXml = newXml;
                        MessageEngine.Global.Trace(Severity.Debug, "New Node: {0}", node.OuterXml);
                    }
                    else
                    {
                        MessageEngine.Global.Trace(Severity.Error, "Template parameters do not match up.  Contains {0} but the template requires {1}", parameterCount, t.MapDictionary.Keys.Count);
                    }
                }
                else
                {
                    MessageEngine.Global.Trace(Severity.Error, "Invalid template {0}", templateName);
                }

            }
            string savePath = vc.Save();
            MessageEngine.Global.Trace(Severity.Notification, "Saved new VulcanConfig to {0}", savePath);
            vc = new VulcanConfig(savePath);
            return vc;
        }
        */

        public Template this[string key]
        {
            get { return templateDictionary[key]; }
        }

        public Dictionary<string, Template> TemplateDictionary
        {
            get { return this.templateDictionary; }
        }
    }

    public class Template
    {
        private string _templateName;
        private string _templateData;
        private string _templateType;

        private Dictionary<string, int> _mapDictionary;

        public Template(string name, string type, string data)
        {
            this._mapDictionary = new Dictionary<string, int>();
            this._templateName = name;
            this._templateType = type;
            this._templateData = data;
        }

        public Dictionary<string, int> MapDictionary
        {
            get { return this._mapDictionary; }
        }

        public string Name
        {
            get { return this._templateName; }
        }

        public string TemplateType
        {
            get { return this._templateType; }
        }

        public string Data
        {
            get { return this._templateData; }
        }
    }
}