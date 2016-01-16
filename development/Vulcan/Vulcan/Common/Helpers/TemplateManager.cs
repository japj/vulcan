/*
 * Microsoft Public License (Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

using Vulcan.Emitters;

namespace Vulcan.Common.Templates
{
    public class TemplateManager
    {
        private XmlSchemaSet _templateSchemaSet;
        private Dictionary<string, Template> templateDictionary;

        public TemplateManager(string templatePath)
            :
            this()
        {
            Message.Trace(Severity.Debug,"Template Manager: Created with Path {0}", templatePath);
            foreach (string file in Directory.GetFiles(templatePath))
            {
                this.AddTemplateFile(file);
            }
        }
        public TemplateManager()
        {
            _templateSchemaSet = new XmlSchemaSet();
            _templateSchemaSet.Add(null, 
                Properties.Settings.Default.DetegoProjectLocationRoot + Path.DirectorySeparatorChar + Properties.Settings.Default.SubpathTemplateSchemaFile);
            templateDictionary = new Dictionary<string, Template>();

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

            Message.Trace(Severity.Debug,"TemplateManager: Validating {0}", fileName);
            templateDocument.Validate(settings_ValidationEventHandler);
            Message.Trace(Severity.Debug,"TemplateManager: Validation Succeeded");

            XPathNavigator templateNavigator = templateDocument.CreateNavigator();

            XmlNamespaceManager templateNamespaceManager = new XmlNamespaceManager(templateNavigator.NameTable);
            templateNamespaceManager.AddNamespace("tm", "http://schemas.microsoft.com/detego/2007/07/07/VulcanTemplate.xsd");

            foreach (XPathNavigator nav in templateNavigator.Select("//tm:Templates/tm:Template", templateNamespaceManager))
            {
                string templateName = nav.SelectSingleNode("@Name").Value.Trim();
                string templateData = nav.SelectSingleNode("tm:TemplateData", templateNamespaceManager).Value.Trim();
                string templateType = nav.SelectSingleNode("@Type").Value.Trim();

                Template t = new Template(templateName, templateType, templateData);
                Message.Trace(Severity.Debug,"Adding template " + templateName);

                if (templateType == "XMLTemplate")
                {
                    Message.Trace(Severity.Debug,"{0} is an XML Template", templateName);
                }
                foreach (XPathNavigator mapNav in nav.Select("tm:Map", templateNamespaceManager))
                {
                    string source = mapNav.SelectSingleNode("@Source").Value;
                    int index = mapNav.SelectSingleNode("@Index").ValueAsInt;
                    Message.Trace(Severity.Debug,"Mapping input {0} to index {1}", source, index);
                    t.MapDictionary.Add(source, index);
                }

                templateDictionary.Add(templateName, t);
            }
        }

        void settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Message.Trace(Severity.Error,"TemplateManager: Schema Validation Error: " + e.Message);
        }

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
                Message.Trace(Severity.Debug,"Replacing Template {0}", node.OuterXml);
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
                            Message.Trace(Severity.Debug, "Mapping Parameter {0}={1}", node.Name, node.Value);
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
                        Message.Trace(Severity.Debug, "Old Node: {0}", node.OuterXml);
                        node.OuterXml = newXml;
                        Message.Trace(Severity.Debug, "New Node: {0}", node.OuterXml);
                    }
                    else
                    {
                        Message.Trace(Severity.Error, "Template parameters do not match up.  Contains {0} but the template requires {1}", parameterCount, t.MapDictionary.Keys.Count);
                    }
                }
                else
                {
                    Message.Trace(Severity.Error, "Invalid template {0}", templateName);
                }

            }
            string savePath = vc.Save();
            Message.Trace(Severity.Notification,"Saved new VulcanConfig to {0}", savePath);
            vc = new VulcanConfig(savePath);
            return vc;
        }

        public Template this[string key]
        {
            get
            {
                return templateDictionary[key];
            }
        }

        public Dictionary<string, Template> TemplateDictionary
        {
            get
            {
                return this.templateDictionary;
            }
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
            get
            {
                return this._mapDictionary;
            }
        }

        public string Name
        {
            get
            {
                return this._templateName;
            }
        }

        public string TemplateType
        {
            get
            {
                return this._templateType;
            }
        }

        public string Data
        {
            get
            {
                return this._templateData;
            }
        }
    }
}
