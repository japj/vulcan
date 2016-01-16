using System;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using Vulcan.Utility.Files;

namespace VulcanEngine.Common
{
    /// <summary>
    /// This class is responsible for implementing a handler for AppDomain.AssemblyResolve and
    /// extending Assembly Resolution.  This allows us to resolve referenced assemblies which are
    /// not in the GAC in circumstances where VulcanEngine is being hosted, such as in
    /// a Driver.exe or MSBuild.
    /// </summary>
    public class AppDomainFusionExtension
    {
        private const string DllExtension = ".dll";
        private ResolveEventHandler _resolveHandler;

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public AppDomainFusionExtension()
        {
            _resolveHandler = new ResolveEventHandler(CurrentDomainAssemblyResolve);
            AppDomain.CurrentDomain.AssemblyResolve += _resolveHandler;
        }

        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        ~AppDomainFusionExtension()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= _resolveHandler;
            _resolveHandler = null;
        }

        private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Convert the string name to an AssemblyName Object
            var assemblyName = new AssemblyName(args.Name);
            string path = PathManager.ToolPath + Path.DirectorySeparatorChar;

            // If Assembly.LoadFile is null, the Assembly Loader will throw the 
            // proper exceptions so this is the correct semantic.
            return Assembly.LoadFile(path + assemblyName.Name + DllExtension);
        }
    }
}
