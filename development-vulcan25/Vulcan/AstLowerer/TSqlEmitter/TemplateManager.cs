using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using AstFramework;
using AstLowerer.Properties;
using VulcanEngine.Common;

namespace AstLowerer.TSqlEmitter
{
    public class TemplateManager
    {
        private readonly XmlSchemaSet _templateSchemaSet;
        private readonly Dictionary<string, Template> _templateDictionary;

        public TemplateManager()
        {
            var assembly = Assembly.GetExecutingAssembly();
            _templateSchemaSet = new XmlSchemaSet();
            _templateSchemaSet.Add(null, XmlReader.Create(assembly.GetManifestResourceStream("AstLowerer.TSqlEmitter.Content.VulcanTemplate.xsd")));
            _templateDictionary = new Dictionary<string, Template>();

            ////MessageEngine.Trace(Severity.Debug, "Template Manager: Created with Path {0}", templatePath);
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.StartsWith("AstLowerer.TSqlEmitter.Content.", StringComparison.InvariantCulture))
                {
                    AddTemplateFile(assembly.GetManifestResourceStream(resourceName));
                }
            }
        }

        public void AddTemplate(string templateName, Template template)
        {
            _templateDictionary.Add(templateName, template);
        }

        public void AddTemplateFile(Stream stream)
        {
            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = _templateSchemaSet };
            settings.ValidationEventHandler += Settings_ValidationEventHandler; 
            
            AddTemplateFile(XmlReader.Create(stream, settings));
        }

        public void AddTemplateFile(string fileName)
        {
            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = _templateSchemaSet };
            settings.ValidationEventHandler += Settings_ValidationEventHandler; 
            AddTemplateFile(XmlReader.Create(fileName, settings));
        }

        public void AddTemplateFile(XmlReader templateXmlReader)
        {
            var templateDocument = new XmlDocument();
            templateDocument.Load(templateXmlReader);

            ////MessageEngine.Trace(Severity.Debug, "TemplateManager: Validating {0}", fileName);
            templateDocument.Validate(Settings_ValidationEventHandler);
            MessageEngine.Trace(Severity.Debug, "TemplateManager: Validation Succeeded");

            XPathNavigator templateNavigator = templateDocument.CreateNavigator();

            var templateNamespaceManager = new XmlNamespaceManager(templateNavigator.NameTable);
            templateNamespaceManager.AddNamespace("tm", "http://schemas.microsoft.com/detego/2007/07/07/VulcanTemplate.xsd");

            foreach (XPathNavigator nav in templateNavigator.Select("//tm:Templates/tm:Template", templateNamespaceManager))
            {
                string templateName = nav.SelectSingleNode("@Name").Value.Trim();
                string templateData = nav.SelectSingleNode("tm:TemplateData", templateNamespaceManager).Value.Trim();
                string templateType = nav.SelectSingleNode("@Type").Value.Trim();

                var t = new Template(templateName, templateType, templateData);
                MessageEngine.Trace(Severity.Debug, "Adding template " + templateName);

                if (templateType == "XMLTemplate")
                {
                    MessageEngine.Trace(Severity.Debug, "{0} is an XML Template", templateName);
                }

                foreach (XPathNavigator mapNav in nav.Select("tm:Map", templateNamespaceManager))
                {
                    string source = mapNav.SelectSingleNode("@Source").Value;
                    int index = mapNav.SelectSingleNode("@Index").ValueAsInt;
                    MessageEngine.Trace(Severity.Debug, "Mapping input {0} to outputPathIndex {1}", source, index);
                    t.MapDictionary.Add(source, index);
                }

                _templateDictionary.Add(templateName, t);
            }
        }

        private void Settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            MessageEngine.Trace(Severity.Error, "TemplateManager: Schema Validation Error: " + e.Message);
        }

        public Template this[string key]
        {
            get { return _templateDictionary[key]; }
        }

        public Dictionary<string, Template> TemplateDictionary
        {
            get { return _templateDictionary; }
        }
    }
}