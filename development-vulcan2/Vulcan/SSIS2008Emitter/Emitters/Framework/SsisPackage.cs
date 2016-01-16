using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Xml;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Task;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.Emitters.Common;
using Ssis2008Emitter.Emitters.Task;


namespace Ssis2008Emitter.Emitters.Framework
{
    [PhysicalIRMapping(typeof(Package))]
    public class SsisPackage : SsisContainer
    {
        public static SsisPackage CurrentPackage = null;

        private SsisProject _projectManager;
        private DTS.Application _DTSApplication;
        private DTS.Package _DTSPackage;

        #region Public Accessors
        public override string Name
        {
            get { return LogicalPackage.Name; }
        }

        public Package LogicalPackage
        {
            get { return (Package)_logicalContainer; }
        }

        public string ProjectFolder
        {
            get { return Path.GetDirectoryName(PackagePath); }
        }

        public string PackagePath
        {
            get
            {
                string packageFile = PathManager.AddSubpath(PackageDirectory, String.Format("{0}.{1}", Name, Resources.ExtensionDTSXPackageFile));
                
                return packageFile;
            }
        }

        public string OutputBaseDirectory
        {
            get
            {
                return PathManager.TargetPath;
            }
        }

        public string PackageDirectory
        {
            get
            {
                return PathManager.AddSubpath(OutputBaseDirectory,PackageRelativeDirectory);
            }
        }

        public string PackageRelativeDirectory
        {
            get
            {
                return getSubpathFromPackageType(this.LogicalPackage.Type) + Path.DirectorySeparatorChar + Name;
            }
        }

        public DTS.Package DTSPackage
        {
            get { return _DTSPackage; }
            private set { _DTSPackage = value; }
        }

        public SsisProject ProjectManager
        {
            get { return _projectManager; }
        }
        #endregion  // Public Accessors

        public SsisPackage(Package logicalPackage, SSISEmitterContext context) : base (logicalPackage, context)
        {
            _DTSApplication = new DTS.Application();
            _DTSPackage = new DTS.Package();
            _DTSPackage.Name = Name.Replace(".", "_").Replace("[", " ").Replace("]", " ");
            _projectManager = new SsisProject(this);

            CurrentPackage = this;
        }

        public override SSISEmitterContext Emit()
        {
            this.UnSave();
            this.Save();

            SSISEmitterContext newContext = null;

            try
            {
                Package logicalPackage = (Package)_logicalContainer;
                this.DTSPackage.Variables.Add("_vulcanPackageRepositoryDirectory", false, "User", OutputBaseDirectory);

                foreach (ConnectionConfiguration connection in logicalPackage.ConnectionConfigurationList)
                {
                    this.AddConfiguration(connection.Name);
                    SsisConnection s = new SsisConnection(connection);
                }

                foreach (PackageConfiguration config in logicalPackage.PackageConfigurationList)
                {
                    this.AddConfiguration(config.Name);
                }

                foreach (Variable variable in logicalPackage.VariableList)
                {
                    SsisVariable s = new SsisVariable(variable);
                }

                this.Reload();

                newContext = new SSISEmitterContext(this, new SsisSequence(this.DTSPackage, _logicalContainer), _context.PluginLoader);
                this.DTSPackage.TransactionOption = (Microsoft.SqlServer.Dts.Runtime.DTSTransactionOption)
                    Enum.Parse(typeof(Microsoft.SqlServer.Dts.Runtime.DTSTransactionOption), this.LogicalPackage.TransactionMode);
                EmitPatterns(newContext);
                this.Save();

            }
            catch (Exception e)
            {
                this.Save();
                MessageEngine.Global.Trace(Severity.Error, e, e.Message);
            }

            return newContext;
        }

        private void UnSave()
        {
            try
            {
                DirectoryInfo packageDirectoryInfo = System.IO.Directory.CreateDirectory(ProjectFolder);
                if (packageDirectoryInfo.Exists)
                {
                    try
                    {
                        Directory.Delete(packageDirectoryInfo.FullName, true);
                    }
                    catch(IOException)
                    {
                    }

                    MessageEngine.Global.Trace(Severity.Alert, "Deleted ETL Folder {0}", packageDirectoryInfo.FullName);
                }
            }
            catch (Exception e)
            {
                MessageEngine.Global.Trace(Severity.Alert, "Error deleting ETL Folder {0}", e.Message);
            }
        }

        private void Save()
        {
            _DTSApplication.UpdatePackage = true;
            _DTSApplication.UpdateObjects = true;

            DirectoryInfo packageDirectoryInfo = System.IO.Directory.CreateDirectory(ProjectFolder);
            _DTSApplication.SaveToXml(PackagePath, DTSPackage, null);
            ProjectManager.Save();
            MessageEngine.Global.Trace(Severity.Alert, "Saved DTS package {0}", PackagePath);
        }

        private void Reload()
        {
            this.Save();
            this.DTSPackage = _DTSApplication.LoadPackage(PackagePath, null);
        }

        private string getSubpathFromPackageType(string packageType)
        {
            string subPath = "";
            switch (packageType)
            {
                case "ETL": subPath = Settings.Default.SubpathETLProjectLocation; break;
                case "Dimension": subPath = Settings.Default.SubpathDimensionProjectLocation; break;
                case "FactTable": subPath = Settings.Default.SubpathFactTableProjectLocation; break;
                case "DataWarehouseInit": subPath = Settings.Default.SubpathDataWarehouseInitProjectLocation; break;
                case "Test": subPath = Settings.Default.SubpathTestProjectLocation; break;
                case "Table": subPath = Settings.Default.SubpathTableProjectLocation; break;
                default: subPath = packageType; MessageEngine.Global.Trace(Severity.Alert, "Unknown project type {0}. Setting subPath to {1}", packageType, subPath); break;
            }
            return subPath;
        }

        private void AddConfiguration(string configurationName)
        {
            DTSPackage.EnableConfigurations = true;

            string packageRoot =
                String.IsNullOrEmpty(VulcanEngine.Common.PathManager.PackageConfigurationPath)
                ? Settings.Default.DetegoPackageConfigurationRoot
                : VulcanEngine.Common.PathManager.PackageConfigurationPath;

            string configFilePath =
                SSISExpressionPathBuilder.BuildAbsoluteExpressionPath(packageRoot
                                                + Path.DirectorySeparatorChar
                                                + configurationName
                                                + "."
                                                + Resources.ExtensionDtsConfigurationFile);

            MessageEngine.Global.Trace(Severity.Debug, "Adding Configuration File {0}", configFilePath);

            DTS.Configuration config = DTSPackage.Configurations.Add();
            config.ConfigurationType = DTS.DTSConfigurationType.ConfigFile;
            config.Name = configurationName;
            config.Description = configurationName;
            config.ConfigurationString = configFilePath;
        }
    }

}
