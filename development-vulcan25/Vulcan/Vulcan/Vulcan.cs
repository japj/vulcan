using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AstFramework;
using Driver.Properties;
using Vulcan.Utility.Files;
using VulcanEngine.IR;
using VulcanEngine.Kernel;

namespace VulcanEngine.Common
{
    public static class VulcanRuntime
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is the main exception swallower for the application.  This should be replaced with a Watson handler.")]
        public static int Main(string[] args)
        {
            int errorCount = 0;

            MessageEngine.Trace(Severity.Notification, Resources.VulcanStart, Assembly.GetAssembly(typeof(XmlIR)).GetName().Version);
            try
            {
                var workflowLoader = new PhaseWorkflowLoader();
                PhaseWorkflow workflow = workflowLoader.PhaseWorkflowsByName[workflowLoader.DefaultWorkflowName];

                var cmdLineParser = new SimpleCommandLineParser("-/", args);

                if (args.Length == 0 || cmdLineParser["?"] != null)
                {
                    DisplayHelpMenu();
                    return errorCount;
                }

                if (cmdLineParser["t"] != null && cmdLineParser["t"].Count > 0)
                {
                    PathManager.TargetPath = Path.GetFullPath(cmdLineParser["t"][0]); //// +Path.DirectorySeparatorChar;
                }
                else
                {
                    PathManager.TargetPath = Path.GetFullPath(".");
                }

                var xmlIR = new XmlIR();

                foreach (string filename in cmdLineParser.NoSwitchArguments)
                {
                    if (File.Exists(filename))
                    {
                        xmlIR.AddXml(Path.GetFullPath(filename), XmlIRDocumentType.Source, true);
                    }
                    else
                    {
                        MessageEngine.Trace(Severity.Error, new FileNotFoundException("Vulcan File Not Found", filename), Resources.VulcanFileNotFound, filename);
                    }
                }

                IList<string> includedFiles = cmdLineParser["i"];

                if (includedFiles != null)
                {
                    foreach (string filename in includedFiles)
                    {
                        if (File.Exists(filename))
                        {
                            xmlIR.AddXml(Path.GetFullPath(filename), XmlIRDocumentType.Include, true);
                        }
                    }
                }

                workflow.ExecutePhaseWorkflowGraph(xmlIR);
            }
            catch (Exception ex)
            {
                MessageEngine.Trace(Severity.Error, ex, "One or more fatal errors were encountered!");
            }

            errorCount = (MessageEngine.ErrorCount + MessageEngine.WarningCount) * -1;
            return errorCount;
        } // end main

        private static void DisplayHelpMenu()
        {
            Console.WriteLine(@"
VULCAN [filename] [-t targetpath] [-r root=path] [-i includefile1] [includefile2] [...] [includefileN]

    filename          Specifies the XML file to be processed.
    -t                Specifies the target path for the generated packages and files.
    -i includefiles   The XML files need to be included to process the source XML file.
            ");
        }
    }
}
