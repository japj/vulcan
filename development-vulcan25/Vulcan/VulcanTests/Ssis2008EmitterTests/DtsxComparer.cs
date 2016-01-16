using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests.Ssis2008EmitterTests
{
    [Flags]
    public enum DtsxComparerOptions
    {
        None = 0x0,
        CanonicalizeFiles = 0x1,
        IgnoreDtsIdDifferences = 0x2,
        IgnoreCreatorInformationDifferences = 0x4,
        IgnoreVersionGuidDifferences = 0x8,
        IgnoreVariableValueDifferences = 0x10,
        IgnoreStringWhiteSpaceDifferences = 0x20,
        IgnoreConnectionManagerDifferences = 0x40,
        IgnoreEventIdDifferences = 0x80,
        IgnoreVersionBuildDifferences = 0x100,
        IgnoreConnectionManagerIdDifferences = 0x200,
        IgnoreGuidDifferences = 0x400,
        IgnoreDataflowIdDifferences = 0x800,
        IgnoreLoggingQueryDifferences = 0x1000,
        IgnoreLastModifiedProductVersion = 0x2000,
        CanonicalizePackageConfigurationEnvironmentVariable = 0x4000
    }

    public class DtsxComparer
    {
        public DtsxComparerOptions DtsxComparerOptions { get; set; }

        private readonly XslCompiledTransform _dtsxCanonicalizerTransform;
        private readonly XslCompiledTransform _dtsxRemoveCreatorInformationTransform;
        private readonly XslCompiledTransform _dtsxRemoveVariableValueTransform;
        private readonly XslCompiledTransform _dtsxRemoveVersionGuidTransform;
        private readonly XslCompiledTransform _dtsxRemoveConnectionManagerPropertiesTransform;
        private readonly XslCompiledTransform _dtsxRemoveEventIdTransform;
        private readonly XslCompiledTransform _dtsxRemoveVersionBuildTransform;
        private readonly XslCompiledTransform _dtsxRemoveLastModifiedProductVersionTransform;

        private XslCompiledTransform LoadXslCompiledTransform(string resourcePath)
        {
            var transform = new XslCompiledTransform();
            transform.Load(XmlReader.Create(GetType().Assembly.GetManifestResourceStream(resourcePath)));
            return transform;
        }

        public DtsxComparer(DtsxComparerOptions dtsxComparerOptions)
        {
            DtsxComparerOptions = dtsxComparerOptions;

            _dtsxCanonicalizerTransform = LoadXslCompiledTransform("VulcanTests.Xslt.DtsxCanonicalizer.xslt");
            _dtsxRemoveCreatorInformationTransform = LoadXslCompiledTransform("VulcanTests.Xslt.DtsxRemoveCreatorInformation.xslt");
            _dtsxRemoveVariableValueTransform = LoadXslCompiledTransform("VulcanTests.Xslt.DtsxRemoveVariableValue.xslt");
            _dtsxRemoveVersionGuidTransform = LoadXslCompiledTransform("VulcanTests.Xslt.DtsxRemoveVersionGUID.xslt");
            _dtsxRemoveConnectionManagerPropertiesTransform = LoadXslCompiledTransform("VulcanTests.Xslt.DtsxRemoveConnectionManagerProperties.xslt");
            _dtsxRemoveEventIdTransform = LoadXslCompiledTransform("VulcanTests.Xslt.DtsxRemoveEventID.xslt");
            _dtsxRemoveVersionBuildTransform = LoadXslCompiledTransform("VulcanTests.Xslt.DtsxRemoveVersionBuild.xslt");
            _dtsxRemoveLastModifiedProductVersionTransform = LoadXslCompiledTransform("VulcanTests.Xslt.DtsxRemoveLastModifiedProductVersion.xslt");
        }

        private bool HasComparerOption(DtsxComparerOptions dtsxComparerOption)
        {
            return (DtsxComparerOptions & dtsxComparerOption) == dtsxComparerOption;
        }

        public void CompareDtsxContents(string content1, string content2)
        {
            if (HasComparerOption(DtsxComparerOptions.CanonicalizeFiles))
            {
                content1 = Utility.ApplyXsltToString(content1, _dtsxCanonicalizerTransform);
                content2 = Utility.ApplyXsltToString(content2, _dtsxCanonicalizerTransform);
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreCreatorInformationDifferences))
            {
                content1 = Utility.ApplyXsltToString(content1, _dtsxRemoveCreatorInformationTransform);
                content2 = Utility.ApplyXsltToString(content2, _dtsxRemoveCreatorInformationTransform);
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreDtsIdDifferences))
            {
                content1 = PatchDtsIds(XDocument.Parse(content1)).ToString();
                content2 = PatchDtsIds(XDocument.Parse(content2)).ToString();
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreVariableValueDifferences))
            {
                content1 = Utility.ApplyXsltToString(content1, _dtsxRemoveVariableValueTransform);
                content2 = Utility.ApplyXsltToString(content2, _dtsxRemoveVariableValueTransform);
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreVersionGuidDifferences))
            {
                content1 = Utility.ApplyXsltToString(content1, _dtsxRemoveVersionGuidTransform);
                content2 = Utility.ApplyXsltToString(content2, _dtsxRemoveVersionGuidTransform);
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreVersionBuildDifferences))
            {
                content1 = Utility.ApplyXsltToString(content1, _dtsxRemoveVersionBuildTransform);
                content2 = Utility.ApplyXsltToString(content2, _dtsxRemoveVersionBuildTransform);
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreConnectionManagerDifferences))
            {
                content1 = Utility.ApplyXsltToString(content1, _dtsxRemoveConnectionManagerPropertiesTransform);
                content2 = Utility.ApplyXsltToString(content2, _dtsxRemoveConnectionManagerPropertiesTransform);
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreEventIdDifferences))
            {
                content1 = Utility.ApplyXsltToString(content1, _dtsxRemoveEventIdTransform);
                content2 = Utility.ApplyXsltToString(content2, _dtsxRemoveEventIdTransform);
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreDataflowIdDifferences))
            {
                content1 = PatchDataflowColumnIds(XDocument.Parse(content1)).ToString();
                content2 = PatchDataflowColumnIds(XDocument.Parse(content2)).ToString();
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreGuidDifferences))
            {
                content1 = Regex.Replace(content1, "{[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-Z0-9]{12}}", string.Empty, RegexOptions.IgnoreCase);
                content2 = Regex.Replace(content2, "{[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-Z0-9]{12}}", string.Empty);

                content1 = Regex.Replace(content1, "_[a-f0-9]{32}.sql", ".sql");
                content2 = Regex.Replace(content2, "_[a-f0-9]{32}.sql", ".sql");

                content1 = Regex.Replace(content1, "_[a-f0-9]{32}", "__PLACEHOLDER__");
                content2 = Regex.Replace(content2, "_[a-f0-9]{32}", "__PLACEHOLDER__");
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreStringWhiteSpaceDifferences))
            {
                content1 = Regex.Replace(content1, @"><", "> <");
                content2 = Regex.Replace(content2, @"><", "> <");

                content1 = Regex.Replace(content1, @"\s+", " ");
                content2 = Regex.Replace(content2, @"\s+", " ");
            }

            if (HasComparerOption(DtsxComparerOptions.IgnoreLastModifiedProductVersion))
            {
                content1 = Utility.ApplyXsltToString(content1, _dtsxRemoveLastModifiedProductVersionTransform);
                content2 = Utility.ApplyXsltToString(content2, _dtsxRemoveLastModifiedProductVersionTransform);
            }

            if (HasComparerOption(DtsxComparerOptions.CanonicalizePackageConfigurationEnvironmentVariable))
            {
                string packageConfigRoot = Environment.GetEnvironmentVariable("VULCAN_PACKAGECONFIGROOT");
                if (!string.IsNullOrEmpty(packageConfigRoot))
                {
                    Regex r = new Regex(Regex.Escape(@"C:\PackageConfigurations"), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    content1 = content1.Replace(packageConfigRoot, "__PLACEHOLDER__");
                    content2 = r.Replace(content2, "__PLACEHOLDER__");
                }
            }

            if (content1 != content2)
            {
                int firstMismatchIndex = GetFirstMismatchIndex(content1, content2);
                string mismatchSubstring1 = GetAdjustedMismatchSubstring(content1, firstMismatchIndex);
                string mismatchSubstring2 = GetAdjustedMismatchSubstring(content2, firstMismatchIndex);
                Assert.AreEqual(mismatchSubstring1, mismatchSubstring2, "Dtsx contents do not match.");
            }
        }

        private static int GetFirstMismatchIndex(string content1, string content2)
        {
            int mismatchIndex = 0;
            while (mismatchIndex < content1.Length && mismatchIndex < content2.Length && content1[mismatchIndex] == content2[mismatchIndex])
            {
                ++mismatchIndex;
            }

            return mismatchIndex;
        }

        private static string GetAdjustedMismatchSubstring(string content, int firstMismatchIndex)
        {
            int adjustedIndex = content.Substring(0, firstMismatchIndex + 1).LastIndexOf("<", StringComparison.Ordinal);
            int adjustedEndIndex = content.IndexOf(">", content.IndexOf(">", adjustedIndex + 1, StringComparison.Ordinal) + 1, StringComparison.Ordinal);
            return content.Substring(adjustedIndex, adjustedEndIndex - adjustedIndex);
        }

        private static XDocument PatchDtsIds(XDocument document)
        {
            var dtsIdDictionary = new Dictionary<string, string>();
            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(new NameTable());
            xmlnsManager.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");
            foreach (var dtsIdElement in document.XPathSelectElements("//DTS:Property[@DTS:Name='DTSID']", xmlnsManager))
            {
                XElement nameElement = dtsIdElement.Parent.XPathSelectElement("./DTS:Property[@DTS:Name='ObjectName']", xmlnsManager);
                if (nameElement != null)
                {
                    dtsIdDictionary.Add(dtsIdElement.Value, nameElement.Value);
                    dtsIdElement.Value = nameElement.Value;
                }
                else
                {
                    dtsIdElement.Value = "__GUID__";
                }
            }

            foreach (XAttribute refAttribute in (IEnumerable)document.XPathEvaluate("//@IDREF"))
            {
                refAttribute.Value = dtsIdDictionary[refAttribute.Value];
            }

            foreach (XElement connectionRef in (IEnumerable)document.XPathEvaluate("//DTS:Executable//DTS:ObjectData//ExecutePackageTask//Connection", xmlnsManager))
            {
                connectionRef.Value = dtsIdDictionary[connectionRef.Value];
            }

            foreach (XAttribute connectionRef in (IEnumerable)document.XPathEvaluate("//connection/@connectionManagerID"))
            {
                connectionRef.Value = dtsIdDictionary[connectionRef.Value];
            }

            return document;
        }

        private static XDocument PatchDataflowColumnIds(XDocument document)
        {
            var dtsIdDictionary = new Dictionary<string, string>();
            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(new NameTable());
            xmlnsManager.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");

            var columnIdXPath = "//component/@id | //input/@id | //inputColumn/@id | //inputColumn/@lineageId | //output/@id | //output/@synchronousInputId | "
                              + "//outputColumn/@id | //outputColumn/@lineageId | //property/@id | //path/@id | //externalMetadataColumn/@id | //connection/@id | "
                              + "//inputColumn/@externalMetadataColumnId";

            foreach (XAttribute dtsIdElement in (IEnumerable)document.XPathEvaluate(columnIdXPath, xmlnsManager))
            {
                XAttribute nameAttribute = dtsIdElement.Parent.Attribute("name");
                if (nameAttribute != null)
                {
                    if (!dtsIdDictionary.ContainsKey(dtsIdElement.Value))
                    {
                        dtsIdDictionary.Add(dtsIdElement.Value, nameAttribute.Value);
                    }

                    dtsIdElement.Value = nameAttribute.Value;
                }
                else
                {
                    dtsIdElement.Value = "__GUID__";
                }
            }

            foreach (XAttribute refAttribute in (IEnumerable)document.XPathEvaluate("//path/@startId | //path/@endId | //outputColumn/@externalMetadataColumnId"))
            {
                refAttribute.Value = dtsIdDictionary[refAttribute.Value];
            }

            foreach (XElement refElement in (IEnumerable)document.XPathEvaluate("//property[@name='Expression'] | //property[@name='ParameterMap']"))
            {
                foreach (Match exp in Regex.Matches(refElement.Value, @"\#[0-9]*"))
                {
                    string id = exp.Value.Substring(1);
                    refElement.Value = refElement.Value.Replace(id, dtsIdDictionary[id]);
                }
            }

            foreach (XElement refElement in (IEnumerable)document.XPathEvaluate("//property[@name='OutputColumnLineageID'] | //property[@name='CustomLineageID'] | //property[@name='SortColumnId']"))
            {
                refElement.Value = dtsIdDictionary[refElement.Value];
            }

            return document;
        }
    }
}