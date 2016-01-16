using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using AstFramework;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.Properties;
using VulcanEngine.Common;

namespace Ssis2008Emitter.IR.Common
{
    public class SsisProject
    {
        private XmlDocument _projectXmlDocument;
        private Collection<string> _miscFiles;
        private Collection<Package> _packages;
        private string _projectPath;

        public SsisProject(string projectPath)
        {
            MessageEngine.Trace(Severity.Debug, "Project Manager: Created Project {0}", projectPath);
            _packages = new Collection<Package>();
            _miscFiles = new Collection<string>();
            _projectXmlDocument = new XmlDocument();
            _projectPath = projectPath;
        }

        public void Save()
        {
            _projectXmlDocument.Load(GetType().Assembly.GetManifestResourceStream("Ssis2008Emitter.Ssis2008Emitter.Content.DataTransformationsProject.dtproj"));
            if (_projectXmlDocument != null)
            {
                XPathNavigator templateNav = _projectXmlDocument.CreateNavigator().SelectSingleNode("//Project");

                templateNav.SelectSingleNode("Database").AppendChildElement(String.Empty, "Name", String.Empty, "placeholder.database");
                templateNav.SelectSingleNode("Database").AppendChildElement(String.Empty, "FullPath", String.Empty, "placeholder.database");

                foreach (var package in _packages)
                {
                    XPathNavigator packageNav = templateNav.SelectSingleNode("DTSPackages");
                    packageNav.PrependChildElement(String.Empty, "DtsPackage", String.Empty, String.Empty);
                    packageNav.MoveToFirstChild();
                    packageNav.AppendChildElement(String.Empty, "Name", String.Empty, String.Format(CultureInfo.InvariantCulture, "{0}.{1}", package.Name, Resources.ExtensionDTSXPackageFile));
                    packageNav.AppendChildElement(String.Empty, "FullPath", String.Empty, String.Format(CultureInfo.InvariantCulture, "{0}.{1}", package.Name, Resources.ExtensionDTSXPackageFile));
                    packageNav.AppendChildElement(String.Empty, "References", String.Empty, String.Empty);
                }

                XPathNavigator miscFilesNav = templateNav.SelectSingleNode("Miscellaneous");
                foreach (string s in _miscFiles)
                {
                    miscFilesNav.PrependChildElement(String.Empty, "ProjectItem", String.Empty, String.Empty);
                    miscFilesNav.MoveToFirstChild();
                    miscFilesNav.AppendChildElement(String.Empty, "Name", String.Empty, s);
                    miscFilesNav.AppendChildElement(String.Empty, "FullPath", String.Empty, s);
                    miscFilesNav.MoveToParent();
                }

                _projectXmlDocument.Save(_projectPath);
            }
        }

        public Collection<Package> Packages
        {
            get { return _packages; }
        }

        public Collection<string> MiscFiles
        {
            get { return _miscFiles; }
        }
    }
}
