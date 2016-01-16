using System;
using System.Resources;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;

using Driver.Properties;
using VulcanEngine.Kernel;
using VulcanEngine.Phases;
using VulcanEngine.IR;

namespace VulcanEngine.Common
{
    public class Engine
    {
        public Engine() { }


        [Conditional("DEBUG")]
        public void ThrowException(Exception e)
        {
            Console.Error.WriteLine(e.StackTrace);
            throw e;
        }

        public void HandleException(Exception e)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Error : {0}", e.Message);
            Console.ForegroundColor = color;
        }
    }

    public static class VulcanRuntime
    {
        public static int Main(string[] args)
        {
            int errorCount = 0;

            MessageEngine Message = MessageEngine.Create("__VULCAN MAIN");

            Message.Trace(Severity.Notification, Resources.VulcanStart + Assembly.GetAssembly(typeof(XmlIR)).GetName().Version);

#if DEBUG
            Message.Trace(Severity.Notification, "DEBUG VERSION");
#endif

            // TODO: Can we set permissions so that only PhaseworkflowLoader can see the workflow editing methods?  (Vsabella: yes you can if you make it its own dll and mark the classes internal)
            PhaseWorkflowLoader WorkflowLoader = new PhaseWorkflowLoader();
            PhaseWorkflow Workflow = WorkflowLoader.PhaseWorkflowsByName[WorkflowLoader.DefaultWorkflowName];

            SimpleCommandLineParser cmdLineParser = new SimpleCommandLineParser("-/", args);

            if (args.Length == 0 || cmdLineParser["?"] != null)
            {
                DisplayHelpMenu();
                return errorCount;
            }

            if (cmdLineParser["t"] != null && cmdLineParser["t"].Count > 0)
            {
                PathManager.TargetPath = Path.GetFullPath(cmdLineParser["t"][0]);// +Path.DirectorySeparatorChar;
            }
            else
            {
                PathManager.TargetPath = Path.GetFullPath(".");
            }

            XmlIR xmlIR = new XmlIR();

            if (cmdLineParser["r"] != null && cmdLineParser["r"].Count > 0)
            {
                string[] rootPath = cmdLineParser["r"][0].Split(new char[] { '=' });

                if (rootPath != null && rootPath.Length == 2 && Directory.Exists(rootPath[1]))
                {
                    xmlIR.SetDocumentRoot(rootPath[0].ToUpperInvariant(), Path.GetFullPath(rootPath[1]));
                }
            }

            foreach (string filename in cmdLineParser.NoSwitchArguments)
            {
                if (File.Exists(filename))
                {
                    VulcanEngine.IR.XmlIRDocumentType docType = XmlIRDocumentType.SOURCE;
                    xmlIR.AddXml(Path.GetFullPath(filename), docType);
                }
                else
                {
                    Message.Trace(Severity.Error, new FileNotFoundException("Vulcan File Not Found", filename), Resources.VulcanFileNotFound, filename);
                }
            }

            IList<string> includedFiles = cmdLineParser["i"];

            if (includedFiles != null)
            {
                foreach (string filename in includedFiles)
                {
                    if (File.Exists(filename))
                    {
                        VulcanEngine.IR.XmlIRDocumentType docType = XmlIRDocumentType.INCLUDE;
                        xmlIR.AddXml(Path.GetFullPath(filename), docType);
                    }
                }
            }

            Workflow.ExecutePhaseWorkflowGraph(xmlIR);
            errorCount = (MessageEngine.AllEnginesErrorCount + MessageEngine.AllEnginesWarningCount) * -1;
            return errorCount;
        }// end main

        private static void DisplayHelpMenu()
        {
            Console.WriteLine(@"
VULCAN [filename] [-t targetpath] [-r root=path] [-i includefile1] [includefile2] [...] [includefileN]

    filename          Specifies the XML file to be processed.
    -t                Specifies the target path for the generated packages and files.
    -r root=path      Specifies the root path. For example:
                      -r detego=c:\detego
                      sets the 'Detego' root to c:\detego
    -i includefiles   The XML files need to be included to process the source XML file.
                      It's suggested to use '-r' on command line and <Using/> in XML instead.
            ");
        }
    }
}
