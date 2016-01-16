using System;
using System.IO;
using AstFramework;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Properties;
using VulcanEngine.Common;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Framework
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class PackageConfiguration : PhysicalObject
    {
        private static string PackageConfigurationPath
        {
            get
            {
                /// TODO: vsabella: 
                /// Remove System.Environment once command line parsing library is improved.
                /// This is a poor workaround until we can do true settings override 
                /// as phase parameters or a better .config file loader.

                if (PropertyManager.Properties.ContainsKey("PackageConfigurationPath"))
                {
                    return PropertyManager.Properties["PackageConfigurationPath"];
                }

                return Environment.GetEnvironmentVariable("VULCAN_PACKAGECONFIGROOT");
            }
        }

        public PackageConfiguration(string name)
            : base(name)
        {
        }

        public override void Initialize(SsisEmitterContext context)
        {
        }

        public override void Emit(SsisEmitterContext context)
        {
            context.Package.DtsPackage.EnableConfigurations = true;

            string packageRoot =
                String.IsNullOrEmpty(PackageConfigurationPath)
                ? Settings.Default.DetegoPackageConfigurationRoot
                : PackageConfigurationPath;

            string configFilePath = StringManipulation.CleanPath(packageRoot
                                                + Path.DirectorySeparatorChar
                                                + Name
                                                + "."
                                                + Resources.ExtensionDtsConfigurationFile);

            MessageEngine.Trace(Severity.Debug, "Adding Configuration File {0}", configFilePath);
            if (!context.Package.DtsPackage.Configurations.Contains(Name))
            {
                DTS.Configuration config = context.Package.DtsPackage.Configurations.Add();
                config.ConfigurationType = DTS.DTSConfigurationType.ConfigFile;
                config.Name = Name;
                config.Description = Name;
                config.ConfigurationString = configFilePath;
                context.Package.DtsPackage.ImportConfigurationFile(configFilePath);
            }
        }
    }
}
