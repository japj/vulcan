using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using VulcanEngine.Common;
using VulcanEngine.Kernel;
using VulcanEngine.Phases;
using VulcanEngine.IR;
using VulcanEngine.Properties;


namespace VulcanEngine.MSBuild
{
    public class VulcanTask : Microsoft.Build.Utilities.Task
    {
        private string _outputPath;
        private ITaskItem[] _sources;
        private ITaskItem[] _includes;

        [Required]
        public string OutputPath
        {
            get
            {
                return _outputPath;
            }
            set
            {
                _outputPath = value;
            }
        }

        // Directories to create.
        [Required]
        public ITaskItem[] Sources
        {
            get
            {
                return _sources;
            }

            set
            {
                _sources = value;
            }
        }

        public ITaskItem[] Includes
        {
            get
            {
                return _includes;
            }

            set
            {
                _includes = value;
            }
        }


        public override bool Execute()
        {
            MessageEngine.Reset();
            MessageEngine Message = MessageEngine.Create("__VULCAN MAIN");
            Message.Trace(Severity.Notification, Resources.VulcanStart + Assembly.GetExecutingAssembly().GetName().Version);

#if DEBUG
            Message.Trace(Severity.Notification, "DEBUG VERSION");
#endif

            PhaseWorkflowLoader WorkflowLoader = new PhaseWorkflowLoader();
            PhaseWorkflow Workflow = WorkflowLoader.PhaseWorkflowsByName[WorkflowLoader.DefaultWorkflowName];
            PathManager.TargetPath = Path.GetFullPath(OutputPath) + Path.DirectorySeparatorChar;

            XmlIR xmlIR = new XmlIR();

            foreach (ITaskItem item in Sources)
            {
                string filename = item.ItemSpec;
                if (File.Exists(filename))
                {
                    xmlIR.AddXml(Path.GetFullPath(filename),XmlIRDocumentType.SOURCE);
                }
                else
                {
                    Message.Trace(Severity.Error, new FileNotFoundException("Vulcan File Not Found", filename), Resources.VulcanFileNotFound, filename);
                }
            }

            foreach (ITaskItem item in Includes)
            {
                string filename = item.ItemSpec;
                if (File.Exists(filename))
                {
                    xmlIR.AddXml(Path.GetFullPath(filename), XmlIRDocumentType.INCLUDE);
                }
                else
                {
                    Message.Trace(Severity.Error, new FileNotFoundException("Vulcan File Not Found", filename), Resources.VulcanFileNotFound, filename);
                }
            }

            Workflow.ExecutePhaseWorkflowGraph(xmlIR);

            return (MessageEngine.AllEnginesErrorCount + MessageEngine.AllEnginesWarningCount) <= 0;
        }
    }
}

/*



            MessageEngine Message = MessageEngine.Create("__VULCAN MAIN");
            Message.Trace(Severity.Notification, Resources.VulcanStart + Assembly.GetExecutingAssembly().GetName().Version);

            // TODO: Can we set permissions so that only PhaseworkflowLoader can see the workflow editing methods?  (Vsabella: yes you can if you make it its own dll and mark the classes internal)
            PhaseWorkflowLoader WorkflowLoader = new PhaseWorkflowLoader();
            PhaseWorkflow Workflow = WorkflowLoader.PhaseWorkflowsByName[WorkflowLoader.DefaultWorkflowName];

            // TODO: Time to add a bona fide command line parser.  We can probably reuse the same type binder from the parser to do attribute-based switch binding
            //temporary hack to get it to build

*/