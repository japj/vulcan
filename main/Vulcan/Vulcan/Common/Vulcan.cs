/*
 * Microsoft Public License (Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

using System;
using System.Resources;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using Vulcan.Properties;
using Vulcan.Packages;

namespace Vulcan.Common
{
    public class VulcanEngine
    {
        private Templates.TemplateManager _templateManager;
        private Dimensions.DimensionHelper _dimensionsHelper;
        
        public VulcanEngine()
            :
            this(
            Settings.Default.DetegoProjectLocationRoot + Path.DirectorySeparatorChar + Settings.Default.SubpathVulcanTemplateFolder,
            Settings.Default.DetegoProjectLocationRoot + Path.DirectorySeparatorChar + Settings.Default.SubpathDimensionLibrary
            )
        {
        }

        public VulcanEngine(string templateFolder, string dimensionFolder)
        {
            _templateManager = new Templates.TemplateManager(templateFolder);
            _dimensionsHelper = new Dimensions.DimensionHelper(dimensionFolder, _templateManager);
        }

        public MessageEngine Compile(string vulcanFile)
        {
            if (File.Exists(vulcanFile))
            {
                Message.PushMessageEngine(vulcanFile);
                VulcanConfig vulcanConfig = _templateManager.XmlTemplateReplacement(new VulcanConfig(vulcanFile));
                foreach (XPathNavigator nav in vulcanConfig.Navigator.Select("//rc:VulcanConfig/rc:Package", vulcanConfig.NamespaceManager))
                {
                    string name = nav.SelectSingleNode("@Name").Value;
                    string packageType = nav.SelectSingleNode("@PackageType").Value;

                    VulcanPackage vulcanPackage = vulcanPackage = new VulcanPackage(name, packageType, vulcanConfig, _templateManager, nav);
                    try
                    {
                        vulcanPackage.ProcessPackage();
                    }
                    catch (Exception)
                    {
                        if (vulcanPackage != null)
                        {
                            if (Settings.Default.CleanupFilesOnError)
                            {
                                vulcanPackage.UnSave();
                            }
                            else
                            {
                                vulcanPackage.Save();
                                vulcanPackage.Validate();
                            }
                        }
                    }
                }
            }
            else
            {
                Message.Trace(Severity.Error, new FileNotFoundException("Vulcan File Not Found", vulcanFile), "VulcanFile {0} not found", vulcanFile);
            }
            return Message.PopMessageEngine();
        } // end compile
    }

    public static class VulcanRuntime
    {
        public static int Main(string[] args)
        {
            int errorCount = 0;

            Message.Trace(Severity.Notification, Resources.VulcanStart + Assembly.GetExecutingAssembly().GetName().Version);
            if (args.Length > 0)
            {

                string vulcanConfigPath = args[0];
                Message.Trace(Severity.Notification, "Building Vulcan Config File " + vulcanConfigPath);

                VulcanEngine ve = new VulcanEngine();
                if (File.Exists(vulcanConfigPath))
                {
                    MessageEngine me = ve.Compile(vulcanConfigPath);
                    errorCount = (me.ErrorCount + me.WarningCount)* -1;
                }
                else
                {
                    Message.Trace(Severity.Error, "Could not find file {0}", vulcanConfigPath);
                }
            }
            return errorCount;
        }// end main
    }
}
