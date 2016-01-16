using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Framework;

namespace Ssis2008Emitter.Emitters.Framework
{
    public class SsisConnectionConfiguration
    {
        private string _name;
        private string _connectionString;
        private SSISEmitterContext _context;

        public SsisConnectionConfiguration(ConnectionConfiguration connection, SSISEmitterContext context)
        {
            this._name = connection.Name;
            this._connectionString = connection.ConnectionString;
            this._context = context;
        }

        public SSISEmitterContext Emit()
        {
            string dtsConfigHeader = @"<DTSConfiguration>
                                    <DTSConfigurationHeading><DTSConfigurationFileInfo /></DTSConfigurationHeading>";
            string dtsPropertyConfig = @"<Configuration ConfiguredType=""Property"" Path=""\Package.Connections[{0}].Properties[{1}]"" ValueType=""String"">
                                            <ConfiguredValue>{2}</ConfiguredValue>
                                         </Configuration>";
            string dtsConfigFooter = "</DTSConfiguration>";

            string configDirectory = PathManager.GetTargetSubpath(Settings.Default.SubpathPackageConfigurationProjectLocation);
            string configFile = PathManager.AddSubpath(configDirectory, String.Format("{0}.{1}", _name, Resources.ExtensionDtsConfigurationFile));

            // Need to create the Target Directory :)
            Directory.CreateDirectory(configDirectory);
            using (StreamWriter sw = new StreamWriter(configFile,false,Encoding.Unicode))
            {
                sw.Write(dtsConfigHeader);
                sw.Write(dtsPropertyConfig, _name, "ConnectionString", _connectionString);
                sw.Write(dtsConfigFooter);
            }
            return _context;
        }
    }
}
