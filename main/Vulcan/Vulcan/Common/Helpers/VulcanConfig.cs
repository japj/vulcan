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

namespace Vulcan.Common
{
    public class VulcanConfig
    {

        private XmlSchemaSet _vulcanSchemas;
        private XmlNamespaceManager _vulcanNamespaceManager;
        private XmlDocument _vulcanConfigXmlDocument;
        private XPathNavigator _vulcanConfigXPathNavigator;
        private string _configFile;

        public VulcanConfig(string configFile)
        {
            this._configFile = configFile;
            Load();

        }

        public void Load()
        {
            _vulcanSchemas = new XmlSchemaSet();
            _vulcanSchemas.Add(null, 
                Properties.Settings.Default.DetegoProjectLocationRoot + Path.DirectorySeparatorChar +Properties.Settings.Default.SubpathVulcanConfigSchemaFile);

            XmlReaderSettings settings = new XmlReaderSettings();

            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = _vulcanSchemas;
            settings.ValidationEventHandler += new ValidationEventHandler(settings_ValidationEventHandler);

            XmlReader vulcanConfigXmlReader = XmlReader.Create(_configFile, settings);


            _vulcanConfigXmlDocument = new XmlDocument();
            _vulcanConfigXmlDocument.Load(vulcanConfigXmlReader);

            Message.Trace(Severity.Debug,"Validating {0}", ConfigFile);
            _vulcanConfigXmlDocument.Validate(settings_ValidationEventHandler);
            Message.Trace(Severity.Debug,"Validation Succeeded");

            _vulcanConfigXPathNavigator = _vulcanConfigXmlDocument.CreateNavigator();

            _vulcanNamespaceManager = new XmlNamespaceManager(_vulcanConfigXPathNavigator.NameTable);
            _vulcanNamespaceManager.AddNamespace("rc", "http://schemas.microsoft.com/detego/2007/07/07/VulcanConfig.xsd");
        }

        public string Save()
        {
            FileInfo fi = new FileInfo(_configFile);
            string fileName = fi.Name + Properties.Resources.ExtensionTempFile;
            string filePath =
                Properties.Settings.Default.DetegoProjectLocationRoot +
                Path.DirectorySeparatorChar +
                Properties.Settings.Default.SubpathTempFolder +
                Path.DirectorySeparatorChar;

            Directory.CreateDirectory(filePath);

            _vulcanConfigXmlDocument.Save(filePath + fileName);
            return filePath+fileName;
        }
        void settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Message.Trace(Severity.Error, Properties.Resources.VulcanConfigValidationError, _configFile, e.Severity, e.Message);
        }

        public XPathNavigator Navigator
        {
            get
            {
                return this._vulcanConfigXPathNavigator;
            }
        }

        public XmlNamespaceManager NamespaceManager
        {
            get
            {
                return this._vulcanNamespaceManager;
            }
        }

        public string ConfigFile
        {
            get
            {
                return this._configFile;
            }
        }
    }
}
