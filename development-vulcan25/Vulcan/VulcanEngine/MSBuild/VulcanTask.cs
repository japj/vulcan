using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using AstFramework;
using Microsoft.Build.Framework;
using Vulcan.Utility.Files;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.Kernel;
using VulcanEngine.Properties;

namespace VulcanEngine.MSBuild
{
    public class VulcanTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string OutputPath { get; set; }

        public string TemplatePath { get; set; }

        public string VulcanParameters { get; set; }

        [Required]
        public ITaskItem[] Sources { get; set; }

        public ITaskItem[] Includes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is the main exception swallower for the application.  This should be replaced with a Watson handler.")]
        public override bool Execute()
        {
            MessageEngine.MSBuildTask = this;

            MessageEngine.Trace(Severity.Notification, Resources.VulcanStart + Assembly.GetExecutingAssembly().GetName().Version);
            try
            {
                InitializeVulcanParameters();

                var workflowLoader = new PhaseWorkflowLoader();
                PhaseWorkflow workflow = workflowLoader.PhaseWorkflowsByName[workflowLoader.DefaultWorkflowName];
                PathManager.TargetPath = Path.GetFullPath(OutputPath) + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar;

                var xmlIR = new XmlIR();

                if (!String.IsNullOrEmpty(TemplatePath))
                {
                    xmlIR.TemplatePath = Path.GetFullPath(TemplatePath) + Path.DirectorySeparatorChar;
                }

                if (Sources != null)
                {
                    foreach (ITaskItem item in Sources)
                    {
                        string filename = item.ItemSpec;
                        if (File.Exists(filename))
                        {
                            xmlIR.AddXml(Path.GetFullPath(filename), XmlIRDocumentType.Source, true);
                        }
                        else
                        {
                            MessageEngine.Trace(Severity.Error, new FileNotFoundException("Vulcan File Not Found", filename), Resources.VulcanFileNotFound, filename);
                        }
                    }
                }

                if (Includes != null)
                {
                    foreach (ITaskItem item in Includes)
                    {
                        string filename = item.ItemSpec;
                        if (File.Exists(filename))
                        {
                            xmlIR.AddXml(Path.GetFullPath(filename), XmlIRDocumentType.Include, true);
                        }
                        else
                        {
                            MessageEngine.Trace(Severity.Error, new FileNotFoundException("Vulcan File Not Found", filename), Resources.VulcanFileNotFound, filename);
                        }
                    }
                }

                workflow.ExecutePhaseWorkflowGraph(xmlIR);
            }
            catch (Exception e)
            {
                MessageEngine.Trace(Severity.Error, e, "One or more fatal errors were encountered!");
            }

            return (MessageEngine.ErrorCount + MessageEngine.WarningCount) <= 0;
        }

        private void InitializeVulcanParameters()
        {
            if (VulcanParameters != null)
            {
                XDocument parametersDocument = XDocument.Parse(VulcanParameters);

                var parameters = from xElem in parametersDocument.Descendants()
                                 where xElem.HasElements == false
                        select new { Name = xElem.Name.LocalName, xElem.Value, xElem.HasElements };

                Console.WriteLine(parameters.Count());
                foreach (var parameter in parameters)
                {
                    if (!PropertyManager.Properties.ContainsKey(parameter.Name))
                    {
                        PropertyManager.Properties.Add(parameter.Name, parameter.Value);
                    }
                    else
                    {
                        PropertyManager.Properties[parameter.Name] = parameter.Value;
                    }
                }
            }
        }
    }
}
