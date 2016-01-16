using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests.Ssis2008EmitterTests
{
    public static class DtprojComparer
    {
        public static void CompareDtprojContents(string content1, string content2)
        {
            var doc1 = XDocument.Parse(content1);
            var doc2 = XDocument.Parse(content2);

            Assert.AreEqual(ProcessDtprojXDocument(doc1), ProcessDtprojXDocument(doc2), "Processed Dtproj contents do not match.");
        }

        private static string ProcessDtprojXDocument(XDocument document)
        {
            foreach (var miscellaneous in document.Descendants("Miscellaneous"))
            {
                foreach (var projectItem in miscellaneous.Elements("ProjectItem"))
                {
                    foreach (var name in projectItem.Elements("Name"))
                    {
                        name.Value = RemoveSqlGuid(name.Value);
                    }

                    foreach (var fullPath in projectItem.Elements("FullPath"))
                    {
                        fullPath.Value = RemoveSqlGuid(fullPath.Value);
                    }
                }
            }

            return document.ToString();
        }

        public static string RemoveSqlGuid(string fileName)
        {
            return Regex.Replace(fileName, @"_[A-Fa-f0-9]{32}\.sql", ".sql");
        }
    }
}