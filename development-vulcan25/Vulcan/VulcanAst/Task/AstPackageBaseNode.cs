using System;
using System.Globalization;
using AstFramework.Model;
using Vulcan.Utility.Files;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstPackageBaseNode
    {
        private string _explicitPackageFolderSubPath;

        public AstPackageBaseNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        public string PackageTypeSubpath
        {
            get { return PackageType; }
        }

        public string PackageFolderSubpath
        {
            get { return String.IsNullOrEmpty(_explicitPackageFolderSubPath) ? Name : _explicitPackageFolderSubPath; }
            set { _explicitPackageFolderSubPath = value; }
        }

        public string PackageRelativeFolder
        {
            get { return PathManager.AddSubpath(PackageTypeSubpath, PackageFolderSubpath); }
        }

        public string PackageFolder
        {
            get { return PathManager.GetTargetSubpath(PackageRelativeFolder); }
        }

        public string PackageFileName
        {
            get { return String.Format(CultureInfo.CurrentCulture, "{0}.dtsx", Name); }
        }

        public string PackageRelativePath
        {
            get { return PathManager.AddSubpath(PackageRelativeFolder, PackageFileName); }
        }

        public string PackagePath
        {
            get { return PathManager.AddSubpath(PackageFolder, PackageFileName); }
        }
    }
}

