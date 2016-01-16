using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private static readonly string DLL_EXTENSION = ".dll";
        private ResolveEventHandler _resolveHandler;
        public AppDomainFusionExtension()
        {
            _resolveHandler = new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            AppDomain.CurrentDomain.AssemblyResolve += _resolveHandler;
        }

        ~AppDomainFusionExtension()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= _resolveHandler;
            _resolveHandler = null;
        }

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Convert the string name to an AssemblyName Object
            AssemblyName assemblyName = new AssemblyName(args.Name);
            string path = PathManager.GetToolPath() + Path.DirectorySeparatorChar;

            // If Assembly.LoadFile is null, the Assembly Loader will throw the 
            // proper exceptions so this is the correct semantic.
            return Assembly.LoadFile(path + assemblyName.Name + DLL_EXTENSION);
        }
    }
}
