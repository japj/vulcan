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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using System.Text;

using Vulcan.Common;
using Vulcan.Tasks;
using Vulcan.Common.Templates;
using Vulcan.Common.Helpers;
using Vulcan.Patterns;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;


namespace Vulcan.Packages
{
    public class VulcanPackage
    {
        private VulcanConfig _vulcanConfig;
        private TemplateManager _templateManager;
        private ProjectManager _projectManager;

        private string _packageName;
        private string _projectSubpath;

        private DTS.Application _DTSApplication;
        private DTS.Package _package;

        private XPathNavigator _packageNavigator;

        public VulcanPackage(string packageName, string packageType, VulcanConfig vulcanConfig, TemplateManager templateManager, XPathNavigator packageNavigator)
        {
            this._packageName = packageName;
            this._projectSubpath = GetSubpathFromPackageType(packageType);

            this._vulcanConfig = vulcanConfig;
            this._templateManager = templateManager;
            this._projectManager = new ProjectManager(packageName);

            _DTSApplication = new DTS.Application();
            _package = new DTS.Package();
            
            this._package.Name = this._packageName;
            this._packageNavigator = packageNavigator;

            this.UnSave();
        }

        public void ProcessPackage()
        {
            Message.Trace(Severity.Notification, "Package {0}", _packageName);
            ConfigureVariables();
            ConfigurePackage();
            ConfigureConnections();
            this.Reload();

            ConfigurePatterns();

            this.Save();
            this.Validate();
        }

        protected void ConfigurePatterns()
        {
            Pattern p = null;
            foreach (XPathNavigator patternNavigator in _packageNavigator.Select("rc:Patterns", _vulcanConfig.NamespaceManager))
            {
                DTS.Executable previousExec = null;
                foreach (XPathNavigator nav in patternNavigator.SelectChildren(XPathNodeType.Element))
                {
                    p = PatternFactory.ProcessPattern(this, this.DTSPackage, nav, null);
                    if (p != null)
                    {
                        AddPrecedenceConstraint(previousExec, p.FirstExecutableGeneratedByPattern, this.DTSPackage);
                        previousExec = p.LastExecutableGeneratedByPattern;
                    }
                }
            }
        }

        protected void ConfigureVariables()
        {
            foreach (XPathNavigator nav in _packageNavigator.Select("rc:Variable", _vulcanConfig.NamespaceManager))
            {

                String variableName = nav.SelectSingleNode("@Name", _vulcanConfig.NamespaceManager).Value;

                // Might as well let the XML system automaticallyu convert values between types on our behalf.
                switch (nav.SelectSingleNode("rc:Type", _vulcanConfig.NamespaceManager).Value.ToUpperInvariant())
                {
                    case "STRING":
                        AddVariable(variableName, nav.SelectSingleNode("rc:Value", _vulcanConfig.NamespaceManager).Value);
                        break;
                    case "INT32":
                        AddVariable(variableName, nav.SelectSingleNode("rc:Value", _vulcanConfig.NamespaceManager).ValueAsInt);
                        break;
                    case "OBJECT":
                        AddVariable(variableName, new object());
                        break;
                    case "BOOLEAN":
                        AddVariable(variableName, nav.SelectSingleNode("rc:Value", _vulcanConfig.NamespaceManager).ValueAsBoolean);
                        break;
                    default:
                        Message.Trace(Severity.Error,
                            "Failure adding package variables: Variable {0}, Unknown type {1}",
                            variableName,
                            nav.SelectSingleNode("rc:Type", _vulcanConfig.NamespaceManager).Value);
                        break;
                }
            }
        }

        protected void ConfigureConnections()
        {
            foreach (XPathNavigator nav in _packageNavigator.Select("rc:Connection", _vulcanConfig.NamespaceManager))
            {
                Dictionary<string, object> connectionProperties = new Dictionary<string, object>();

                string connectionType = nav.SelectSingleNode("rc:Type", _vulcanConfig.NamespaceManager).Value;
                connectionProperties["Name"] = nav.SelectSingleNode("@Name", _vulcanConfig.NamespaceManager).Value;
                connectionProperties["Description"] = nav.SelectSingleNode("@Name", _vulcanConfig.NamespaceManager).Value;
                connectionProperties["ConnectionString"] = "Data Source=PLACEHOLDER;Provider=SQLNCLI.1;Integrated Security=SSPI;Auto Translate=False;";
                connectionProperties["ServerName"] = "PLACEHOLDER";

                Connection c = AddConnection(connectionType, connectionProperties, null);

                string retainSameConnection = nav.SelectSingleNode("@RetainSameConnection").ValueAsBoolean.ToString();
                c.SetExpression("RetainSameConnection", retainSameConnection);
                // at some point this should be an event driven system, you tell VulcanConfig to create a connection, and it pings back and calls us.
                if (nav.SelectSingleNode("rc:CreateConfiguration", _vulcanConfig.NamespaceManager).ValueAsBoolean == true)
                {
                    AddConfiguration(
                        nav.SelectSingleNode("@Name", _vulcanConfig.NamespaceManager).Value,
                        nav.SelectSingleNode("@Name", _vulcanConfig.NamespaceManager).Value + Properties.Resources.DataSourceQualifier + ".dtsConfig",
                        true);
                }
            }
        }

        protected void ConfigurePackage()
        {
            foreach (XPathNavigator nav in _packageNavigator.Select("rc:PackageConfiguration", _vulcanConfig.NamespaceManager))
            {
                AddConfiguration(
                    nav.SelectSingleNode("@Name", _vulcanConfig.NamespaceManager).Value,
                    nav.SelectSingleNode("rc:Filename", _vulcanConfig.NamespaceManager).Value,
                    nav.SelectSingleNode("rc:CreateNew", _vulcanConfig.NamespaceManager).ValueAsBoolean
                    );
            }
        }

        public Connection AddConnection(string connectionType, Dictionary<string, object> properties, Dictionary<string, string> expressions)
        {
            //empty string is ok for the componentName 
            Connection connection = new Connection(
                                                    this,
                                                    properties["Name"].ToString(),
                                                    properties["Description"].ToString(),
                                                    connectionType,
                                                    null
                                                    );
            connection.SetProperties(properties);
            connection.SetExpressions(expressions);
            return connection;
        }
        public void AddConfiguration(string configurationName, string fileName, bool createNew)
        {
            _package.EnableConfigurations = true;

            string configFilePath = ExpressionPathBuilder.PathCleaner(Settings.Default.DetegoPackageConfigurationRoot
                                                + Path.DirectorySeparatorChar
                                                + fileName);

            Message.Trace(Severity.Debug,"Adding Configuration File {0}", configFilePath);

            if (createNew)
            {
                Message.Trace(Severity.Debug,"XML Says CreateNew: Creating Configuration File {0}", configFilePath);
                _package.ExportConfigurationFile(configFilePath + Resources.ExtensionTempFile);
                
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(configFilePath + Resources.ExtensionTempFile);

                XmlTextWriter xmlPrettyPrinter = new XmlTextWriter(configFilePath, null);
                xmlPrettyPrinter.Formatting = Formatting.Indented;

                xmlDoc.Save(xmlPrettyPrinter);

                xmlPrettyPrinter.Flush();
                xmlPrettyPrinter.Close();
                File.Delete(configFilePath + Resources.ExtensionTempFile);
            }

            DTS.Configuration config = _package.Configurations.Add();
            config.ConfigurationType = DTS.DTSConfigurationType.ConfigFile;
            config.Name = configurationName;
            config.Description = configurationName;
            config.ConfigurationString = configFilePath;
        }

        public DTS.Variable AddVariable(string name, object value)
        {
            if (!DTSPackage.Variables.Contains(name))
            {
                Message.Trace(Severity.Alert, "Creating variable {0} with value {1}", name, value);
                DTS.Variable var = _package.Variables.Add(name, false, "User", value);
                return var;
            }
            else
            {
                return DTSPackage.Variables[name];
            }
        }

        public DTS.Variable GetExistingVariable(string name)
        {
            if (DTSPackage.Variables.Contains(name))
            {
                return DTSPackage.Variables[name];
            }
            else
            {
                return null;
            }
        }

        public DTS.Sequence AddSequenceContainer(string name, DTS.IDTSSequence parentContainer)
        {
            if (name != null && parentContainer != null)
            {

                if (parentContainer.Executables.Contains(name))
                {
                    if (parentContainer.Executables[name] is DTS.IDTSSequence)
                    {
                        return (DTS.Sequence)parentContainer.Executables[name];
                    }
                }
                DTS.Sequence sequence = (DTS.Sequence)parentContainer.Executables.Add("STOCK:Sequence");
                sequence.Name = name;
                return sequence;
            }
            return null;
        }

        public void AddPrecedenceConstraint(DTS.Executable from, DTS.Executable to, DTS.IDTSSequence parentContainer)
        {
            if (from != null && to != null && parentContainer != null)
            {
                if (
                    parentContainer.Executables.Contains(this.ExtractNameFromDTSExecutable(from)) &&
                    parentContainer.Executables.Contains(this.ExtractNameFromDTSExecutable(to))
                   )
                {
                    DTS.PrecedenceConstraint pc =
                                             parentContainer.PrecedenceConstraints.Add(from, to);
                    pc.Name = ExtractNameFromDTSExecutable(from) + "_" + ExtractNameFromDTSExecutable(to);
                }
            }
        }

        public string GetSubpathFromPackageType(string packageType)
        {
            string subPath = "";
            switch (packageType)
            {
                case "ETL":
                    subPath = Settings.Default.SubpathETLProjectLocation;
                    break;
                case "Dimension":
                    subPath = Settings.Default.SubpathDimensionProjectLocation;
                    break;
                case "FactTable":
                    subPath = Settings.Default.SubpathFactTableProjectLocation;
                    break;
                case "DataWarehouseInit":
                    subPath = Settings.Default.SubpathDataWarehouseInitProjectLocation;
                    break;
                case "Test":
                    subPath = Settings.Default.SubpathTestProjectLocation;
                    break;
                default:
                    subPath = packageType;
                    Message.Trace(Severity.Alert, "Unknown project type {0}. Setting subPath to {1}", packageType, subPath);
                    break;
            }
            return subPath;
        }


        public string AddFileToProject(string fileName)
        {
            _projectManager.MiscFiles.Add(fileName);
            return ExpressionPathBuilder.PathCleaner(QualifiedProjectPath + Path.DirectorySeparatorChar + fileName);
        }

        public void Reload()
        {
            this.Save();

            string qualifiedProjectPath = QualifiedProjectPath;
            DirectoryInfo packageDirectoryInfo = System.IO.Directory.CreateDirectory(qualifiedProjectPath);

            qualifiedProjectPath = packageDirectoryInfo.FullName
                                    + _packageName
                                    + Resources.ExtensionDTSXProjectFile;

            this._package = _DTSApplication.LoadPackage(qualifiedProjectPath, null);
        }

        public void UnSave()
        {
            try
            {
                DirectoryInfo packageDirectoryInfo = System.IO.Directory.CreateDirectory(QualifiedProjectPath);
                Directory.Delete(packageDirectoryInfo.FullName, true);

                Message.Trace(Severity.Alert, "Deleted Invalid ETL Folder {0}", packageDirectoryInfo.FullName);
            }
            catch (Exception)
            {
            }
        }

        public void Save()
        {
            _DTSApplication.UpdatePackage = true;
            _DTSApplication.UpdateObjects = true;

            ///TODO: vsabella: move this out to a common utility function
            string qualifiedProjectPath = QualifiedProjectPath;
            DirectoryInfo packageDirectoryInfo = System.IO.Directory.CreateDirectory(qualifiedProjectPath);

            qualifiedProjectPath = packageDirectoryInfo.FullName
                                    + _packageName
                                    + Resources.ExtensionDTSXProjectFile;

            _DTSApplication.SaveToXml(qualifiedProjectPath, _package, null);
            _projectManager.Save(QualifiedProjectPath);
            Message.Trace(Severity.Alert, "Saved DTS package {0}", qualifiedProjectPath);
        }

        public void Validate()
        {
            Common.ErrorEvents errorHandler = new Common.ErrorEvents();
            Message.Trace(Severity.Alert, "Validating {0}", this.Name);
            this._package.Validate(null, null, errorHandler, null);
        }

        public string ExtractNameFromDTSExecutable(DTS.Executable exec)
        {
            PropertyInfo namePropertyInfo = exec.GetType().GetProperty("Name");
            if (namePropertyInfo != null)
            {
                return (string)namePropertyInfo.GetValue(exec, null);
            }
            else
            {
                return "";
            }
        }

        public DTS.Package DTSPackage
        {
            get
            {
                return this._package;
            }
        }

        public string Name
        {
            get
            {
                return this._packageName;
            }
        }

        public VulcanConfig VulcanConfig
        {
            get
            {
                return this._vulcanConfig;
            }
        }

        public TemplateManager TemplateManager
        {
            get
            {
                return this._templateManager;
            }
        }

        public ProjectManager ProjectManager
        {
            get
            {
                return this._projectManager;
            }
        }

        public string ProjectSubpath
        {
            get
            {
                return this._projectSubpath;
            }
        }

        public string QualifiedProjectPath
        {
            get
            {
                
                return Settings.Default.DetegoProjectLocationRoot
                                            + Path.DirectorySeparatorChar
                                            + _projectSubpath
                                            + Path.DirectorySeparatorChar
                                            + _packageName
                                            + Path.DirectorySeparatorChar;
            }
        }
    }
}
