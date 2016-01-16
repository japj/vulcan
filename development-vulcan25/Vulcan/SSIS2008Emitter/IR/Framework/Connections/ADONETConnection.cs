using System;
using System.Globalization;
using System.IO;
using System.Text;
using Ssis2008Emitter.IR.Common;
using Vulcan.Utility.Files;
using VulcanEngine.Common;

namespace Ssis2008Emitter.IR.Framework.Connections
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class AdoNetConnection : Connection
    {
        public AdoNetConnection(VulcanEngine.IR.Ast.Connection.AstConnectionNode astNode) : base(astNode)
        {
            ConnectionType = "ADO.NET:System.Data.SqlClient.SqlConnection, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        }

        public override void Emit(SsisEmitterContext context)
        {
            Initialize(context);

            if (!ReusedExisting)
            {
                string connectionString = ConnectionString;
                SetProperty("RetainSameConnection", RetainSameConnection);
                SetProperty("ConnectionString", connectionString);

                // Need to unwind this and split it out logically.
                const string DtsConfigHeader = @"<DTSConfiguration>
                                    <DTSConfigurationHeading><DTSConfigurationFileInfo /></DTSConfigurationHeading>";
                const string DtsPropertyConfig = @"<Configuration ConfiguredType=""Property"" Path=""\Package.Connections[{0}].Properties[{1}]"" ValueType=""String"">
                                            <ConfiguredValue>{2}</ConfiguredValue>
                                         </Configuration>";
                const string DtsConfigFooter = "</DTSConfiguration>";

                string configDirectory = PathManager.GetTargetSubpath(Properties.Settings.Default.SubpathPackageConfigurationProjectLocation);
                string configFile = PathManager.AddSubpath(configDirectory, String.Format(CultureInfo.InvariantCulture, "{0}.{1}", Name, Properties.Resources.ExtensionDtsConfigurationFile));

                // Need to create the Target Directory :)
                Directory.CreateDirectory(configDirectory);
                using (var sw = new StreamWriter(configFile, false, Encoding.Unicode))
                {
                    sw.Write(DtsConfigHeader);
                    sw.Write(DtsPropertyConfig, Name, "ConnectionString", OriginalConnectionString);
                    sw.Write(DtsConfigFooter);
                    sw.Flush();
                    sw.Close();
                }

                var pc = new PackageConfiguration(Name);
                pc.Emit(context);
            }
        }
    }
}
