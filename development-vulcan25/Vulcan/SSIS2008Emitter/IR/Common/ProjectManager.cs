using System;
using System.Collections.Generic;
using System.Globalization;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.Properties;
using Vulcan.Utility.Files;

namespace Ssis2008Emitter.IR.Common
{
    public class ProjectManager
    {
        private Dictionary<string, SsisProject> _projectDirectoryMappings;

        public SsisProject AddPackage(Package package)
        {
            if (!_projectDirectoryMappings.ContainsKey(package.PackageFolder))
            {
                string projectName = package.PackageFolderSubpath ?? package.Name;
                string projectFolder = package.PackageFolder;
                string projectPath = PathManager.AddSubpath(projectFolder, String.Format(CultureInfo.CurrentCulture, "{0}.{1}", projectName, Resources.ExtensionDTProjectFile));
                _projectDirectoryMappings.Add(package.PackageFolder, new SsisProject(projectPath));
            }

            SsisProject ssisProject = _projectDirectoryMappings[package.PackageFolder];
            ssisProject.Packages.Add(package);
            return ssisProject;
        }

        internal ProjectManager()
        {
            _projectDirectoryMappings = new Dictionary<string, SsisProject>();
        }
    }
}
