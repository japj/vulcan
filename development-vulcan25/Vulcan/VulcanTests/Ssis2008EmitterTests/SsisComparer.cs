using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests.Ssis2008EmitterTests
{
    public class SsisComparer
    {
        private static SsisComparer _defaultSsisComparer;

        public static SsisComparer DefaultSsisComparer
        {
            get
            {
                if (_defaultSsisComparer == null)
                {
                    const DtsxComparerOptions DefaultComparerOptions =
                        DtsxComparerOptions.CanonicalizeFiles | DtsxComparerOptions.IgnoreCreatorInformationDifferences |
                        DtsxComparerOptions.IgnoreDtsIdDifferences | DtsxComparerOptions.IgnoreVariableValueDifferences |
                        DtsxComparerOptions.IgnoreVersionGuidDifferences | DtsxComparerOptions.IgnoreStringWhiteSpaceDifferences |
                        DtsxComparerOptions.IgnoreConnectionManagerDifferences | DtsxComparerOptions.IgnoreEventIdDifferences | 
                        DtsxComparerOptions.IgnoreVersionBuildDifferences | DtsxComparerOptions.IgnoreDataflowIdDifferences | 
                        DtsxComparerOptions.IgnoreGuidDifferences | DtsxComparerOptions.IgnoreLastModifiedProductVersion |
                        DtsxComparerOptions.CanonicalizePackageConfigurationEnvironmentVariable;

                    _defaultSsisComparer = new SsisComparer(DefaultComparerOptions);
                }

                return _defaultSsisComparer;
            }
        }

        public DtsxComparer DtsxComparer { get; set; }

        public SsisComparer(DtsxComparerOptions dtsxComparerOptions)
        {
            DtsxComparer = new DtsxComparer(dtsxComparerOptions);
        }

        public void CompareResourceBimlWithDtsx(string bimlResourceName, string resourceDirectoryName)
        {
            string bimlFilename = Utility.LoadResourceToTempFile("VulcanTests.Ssis2008EmitterTests", bimlResourceName);
            string directoryPath = Utility.LoadResourceDirectoryToTempDirectory("VulcanTests.Ssis2008EmitterTests", resourceDirectoryName);
            CompareBimlWithDirectory(bimlFilename, directoryPath);
        }

        public void CompareBimlWithDirectory(string bimlFileName, string directoryPath)
        {
            Assert.IsTrue(File.Exists(bimlFileName), "PRE file {0} was not found", Path.GetFileName(bimlFileName));
            Assert.IsTrue(Directory.Exists(directoryPath), "POST directory {0} was not found", directoryPath);

            string targetDirectoryName = Path.GetFileNameWithoutExtension(bimlFileName);
            Directory.CreateDirectory(targetDirectoryName);

            RunVulcan(bimlFileName, targetDirectoryName);

            CompareDirectories(Path.GetFullPath(targetDirectoryName), directoryPath);
        }
        
        private void CompareDirectories(string directoryPath1, string directoryPath2)
        {
            var directoryInfo1 = new DirectoryInfo(directoryPath1);
            var directoryInfo2 = new DirectoryInfo(directoryPath2);
            CompareDirectories(directoryInfo1, directoryInfo2);
        }

        private void CompareDirectories(DirectoryInfo directoryInfo1, DirectoryInfo directoryInfo2)
        {
            var childInfos1 = directoryInfo1.GetFileSystemInfos();
            var childInfos2 = directoryInfo2.GetFileSystemInfos();
            Assert.AreEqual(childInfos1.Length, childInfos2.Length, String.Format(CultureInfo.InvariantCulture, "Directories do not have matching child counts: '{0}' and '{1}'", directoryInfo1.Name, directoryInfo2.Name));

            for (int i = 0; i < childInfos1.Length; i++)
            {
                FileSystemInfo currentChildInfo1 = childInfos1[i];
                FileSystemInfo currentChildInfo2 = childInfos2[i];
                Assert.AreEqual(currentChildInfo1.GetType(), currentChildInfo2.GetType(), String.Format(CultureInfo.InvariantCulture, "Directory child types do not match: '{0}'", currentChildInfo1.Name));

                var currentChildDirectoryInfo1 = currentChildInfo1 as DirectoryInfo;
                var currentChildFileInfo1 = currentChildInfo1 as FileInfo;

                if (currentChildDirectoryInfo1 != null)
                {
                    Assert.AreEqual(currentChildInfo1.Name, currentChildInfo2.Name, String.Format(CultureInfo.InvariantCulture, "Directory names do not match: '{0}' and '{1}'", currentChildInfo1.Name, currentChildInfo2.Name));
                    var currentChildDirectoryInfo2 = currentChildInfo2 as DirectoryInfo;
                    CompareDirectories(currentChildDirectoryInfo1, currentChildDirectoryInfo2);
                }
                else if (currentChildFileInfo1 != null)
                {
                    var currentChildFileInfo2 = currentChildInfo2 as FileInfo;
                    if (currentChildFileInfo1.Extension == ".dtsx")
                    {
                        Assert.AreEqual(currentChildInfo1.Name, currentChildInfo2.Name, String.Format(CultureInfo.InvariantCulture, "File names do not match: '{0}' and '{1}'", currentChildInfo1.Name, currentChildInfo2.Name));
                        DtsxComparer.CompareDtsxContents(File.ReadAllText(currentChildFileInfo1.FullName), File.ReadAllText(currentChildFileInfo2.FullName));
                    }
                    else if (currentChildFileInfo1.Extension == ".dtsConfig")
                    {
                        Assert.AreEqual(currentChildInfo1.Name, currentChildInfo2.Name, String.Format(CultureInfo.InvariantCulture, "File names do not match: '{0}' and '{1}'", currentChildInfo1.Name, currentChildInfo2.Name));
                        XmlComparer.CompareText(File.ReadAllText(currentChildFileInfo1.FullName), File.ReadAllText(currentChildFileInfo2.FullName));
                    }
                    else if (currentChildFileInfo1.Extension == ".dtproj")
                    {
                        Assert.AreEqual(currentChildInfo1.Name, currentChildInfo2.Name, String.Format(CultureInfo.InvariantCulture, "File names do not match: '{0}' and '{1}'", currentChildInfo1.Name, currentChildInfo2.Name));
                        DtprojComparer.CompareDtprojContents(File.ReadAllText(currentChildFileInfo1.FullName), File.ReadAllText(currentChildFileInfo2.FullName));
                    }
                    else if (currentChildFileInfo1.Extension == ".sql")
                    {
                        var remappedName1 = DtprojComparer.RemoveSqlGuid(currentChildInfo1.Name);
                        var remappedName2 = DtprojComparer.RemoveSqlGuid(currentChildInfo2.Name);
                        Assert.AreEqual(remappedName1, remappedName2, String.Format(CultureInfo.InvariantCulture, "File name roots do not match: '{0}' and '{1}'", currentChildInfo1.Name, currentChildInfo2.Name));
                        XmlComparer.CompareText(File.ReadAllText(currentChildFileInfo1.FullName), File.ReadAllText(currentChildFileInfo2.FullName));
                    }
                    else
                    {
                        Assert.AreEqual(currentChildInfo1.Name, currentChildInfo2.Name, String.Format(CultureInfo.InvariantCulture, "File names do not match: '{0}' and '{1}'", currentChildInfo1.Name, currentChildInfo2.Name));
                        FileStream stream1 = currentChildFileInfo1.OpenRead();
                        FileStream stream2 = currentChildFileInfo2.OpenRead();
                        Assert.IsTrue(stream1.IsEqualTo(stream2), String.Format(CultureInfo.InvariantCulture, "Files do not match: {0}", currentChildInfo1.Name));
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Test code not exposed externally.")]
        private static void RunVulcan(string bimlFileName, string targetDirectoryPath)
        {
            var vulcanProcess = new Process();
            vulcanProcess.StartInfo.FileName = "vulcan.exe";
            vulcanProcess.StartInfo.UseShellExecute = false;
            vulcanProcess.StartInfo.CreateNoWindow = true;
            vulcanProcess.StartInfo.RedirectStandardOutput = true;
            vulcanProcess.StartInfo.RedirectStandardError = true;
            vulcanProcess.StartInfo.Arguments = "\"" + bimlFileName + "\" -t " + targetDirectoryPath;

            if (vulcanProcess.Start())
            {
                vulcanProcess.WaitForExit();

                Assert.IsTrue(vulcanProcess.ExitCode == 0, "Compilation of PRE file failed.");
            }
        }
    }
}
