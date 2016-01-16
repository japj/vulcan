using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Emitters.Framework;

namespace Ssis2008Emitter.Emitters.Framework
{
    public class SsisProject
    {
        private XmlDocument _projectXmlDocument;
        private Collection<string> _miscFiles;

        private SsisPackage _package;

        public SsisProject(SsisPackage package)
        {
            MessageEngine.Global.Trace(Severity.Debug,"Project Manager: Created Project {0}", package.Name);
            _package = package;

            this._miscFiles = new Collection<string>();
            this._projectXmlDocument = new XmlDocument();
        }

        public void Save()
        {
            string templatePath = PathManager.GetToolSubpath(Settings.Default.SubpathSsis2008EmitterContent);
            this._projectXmlDocument.Load(PathManager.AddSubpath(templatePath, Settings.Default.SubpathDTProjectTemplateFile));
            XPathNavigator templateNav = _projectXmlDocument.CreateNavigator().SelectSingleNode("//Project");

            templateNav.SelectSingleNode("Database").AppendChildElement("","Name","","placeholder.database");
            templateNav.SelectSingleNode("Database").AppendChildElement("", "FullPath", "", "placeholder.database");

            XPathNavigator packageNav = templateNav.SelectSingleNode("DTSPackages");
            packageNav.PrependChildElement("", "DtsPackage", "", "");
            packageNav.MoveToFirstChild();
            packageNav.AppendChildElement("", "Name", "", String.Format("{0}.{1}", _package.Name, Resources.ExtensionDTSXPackageFile));
            packageNav.AppendChildElement("", "FullPath", "", String.Format("{0}.{1}", _package.Name, Resources.ExtensionDTSXPackageFile));
            packageNav.AppendChildElement("", "References", "", "");

            XPathNavigator miscFilesNav = templateNav.SelectSingleNode("Miscellaneous");
            foreach (string s in _miscFiles)
            {
                miscFilesNav.PrependChildElement("", "ProjectItem", "", "");
                miscFilesNav.MoveToFirstChild();
                miscFilesNav.AppendChildElement("", "Name", "", s);
                miscFilesNav.AppendChildElement("", "FullPath", "", s);
                miscFilesNav.MoveToParent();
            }

            _projectXmlDocument.Save(Path.ChangeExtension(_package.PackagePath, Resources.ExtensionDTProjectFile));

        }

        public Collection<string> MiscFiles
        {
            get { return this._miscFiles; }
        }
    }
}
