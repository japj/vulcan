using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace VulcanTests
{
    public static class Utility
    {
        public static string LoadResourceToTempFile(string resourcePrefix, string resourceName)
        {
            string filename = String.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", resourcePrefix, resourceName, Path.GetExtension(resourceName));
            string resourcePath = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", resourcePrefix, resourceName);
            LoadResourcePathToFilePath(resourcePath, filename);
            return Path.GetFullPath(filename);
        }

        public static void LoadResourcePathToFilePath(string resourcePath, string filePath)
        {
            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (resourceStream != null)
            {
                using (var outputFileStream = new FileStream(filePath, FileMode.CreateNew))
                {
                    resourceStream.CopyTo(outputFileStream);
                }
            }
        }

        public static void CleanTempFile(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            fileInfo.Delete();
        }

        public static string LoadResourceDirectoryToTempDirectory(string resourcePrefix, string resourceDirectoryName)
        {
            string resourceDirectoryPath = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", resourcePrefix, resourceDirectoryName);
            ////string directoryName = String.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", resourcePrefix, resourceDirectoryName, Guid.NewGuid());
            //// Shortening path name
            string directoryName = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", resourcePrefix, resourceDirectoryName);
            string directoryPath = Path.GetFullPath(directoryName);
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resourcePath in assembly.GetManifestResourceNames())
            {
                if (resourcePath.StartsWith(resourceDirectoryPath, StringComparison.InvariantCulture))
                {
                    string subPath = String.Empty;
                    List<string> resourceSubDirectories = resourcePath.Substring(resourceDirectoryPath.Length).Split('.').ToList();
                    while (resourceSubDirectories.Count > 2)
                    {
                        // TODO: Does this create a root path?
                        subPath = Path.Combine(subPath, resourceSubDirectories[0]);
                        Directory.CreateDirectory(Path.Combine(directoryPath, subPath));
                        resourceSubDirectories.RemoveAt(0);
                    }

                    LoadResourcePathToFilePath(resourcePath, Path.Combine(Path.Combine(directoryPath, subPath), String.Format(CultureInfo.InvariantCulture, "{0}.{1}", resourceSubDirectories[0], resourceSubDirectories[1])));
                }
            }

            return directoryPath;
        }

        public static void CleanTempDirectory(string directoryName)
        {
            var directoryInfo = new DirectoryInfo(directoryName);
            directoryInfo.Delete();
        }

        public static string ApplyXsltToString(string source, XslCompiledTransform xslt)
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                xslt.Transform(XmlReader.Create(new StringReader(source)), null, writer);
            }

            return builder.ToString();
        }
    }
}
