using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Reflection;
using System.IO;

using VulcanEngine.Common;
using VulcanEngine.Properties;

namespace VulcanEngine.Kernel
{
    public class PhaseWorkflowLoader
    {
        private string _defaultWorkflowName;
        private PhasePluginLoader _phasePluginLoader;
        private Dictionary<string, PhaseWorkflow> _phaseWorkflowsByName;

        private MessageEngine Message;

        public Dictionary<string, PhaseWorkflow> PhaseWorkflowsByName
        {
            get { return _phaseWorkflowsByName; }
        }

        public string DefaultWorkflowName
        {
            get { return _defaultWorkflowName; }
        }

        public PhaseWorkflowLoader()
        {
            this._phasePluginLoader = new PhasePluginLoader();
            this._phaseWorkflowsByName = new Dictionary<string, PhaseWorkflow>();

            _loadWorkflows();
        }

        private void _loadWorkflows()
        {
            string phaseWorkflowDescriptorPath = PathManager.GetToolSubpath(Settings.Default.SubpathDefaultPhaseWorkflowFile);

            this.Message = MessageEngine.Create(String.Format("__PHASE WORKFLOW LOADER: {0}", phaseWorkflowDescriptorPath));
            
            XDocument doc = XDocument.Load(phaseWorkflowDescriptorPath);
            
            //TODO: VSABELLA: REVIEW Remove hardcode schema for now, and load it from the document itself with XPathNavigator.GetNamespaceInScope.  
            XNamespace ns = "http://schemas.microsoft.com/detego/2007/07/07/VulcanPhaseWorkflows.xsd";

            this._defaultWorkflowName = doc.Root.Attribute("DefaultWorkflow").Value;
            var phaseWorkflows = from pw in doc.Descendants(ns + "PhaseWorkflow")
                                 select new
                                 {
                                     Name = pw.Attribute("Name").Value,
                                     Phases = (from phase in pw.Descendants(ns + "Phase")
                                               select new
                                               {
                                                   Name = phase.Attribute("PhaseName").Value,
                                                   WorkflowUniqueName = phase.Attribute("WorkflowUniqueName").Value,
                                                   Parameters = (from param in phase.Descendants(ns + "Parameter")
                                                                 select new
                                                                 {
                                                                     Name = param.Attribute("Name").Value,
                                                                     Type = param.Attribute("Type").Value,
                                                                     Value = param.Value
                                                                 })
                                               }),

                                     IRVectors = (from irVector in pw.Descendants(ns+"IRFlowVector")
                                                  select new
                                                  {
                                                      SourceWorkflowUniqueName = irVector.Attribute("SourceWorkflowUniqueName").Value,
                                                      SinkWorkflowUniqueName = irVector.Attribute("SinkWorkflowUniqueName").Value
                                                  })
                                 };

            foreach (var phaseWorkflow in phaseWorkflows)
            {
                if (!this._phaseWorkflowsByName.ContainsKey(phaseWorkflow.Name))
                {
                    PhaseWorkflow workflow = new PhaseWorkflow(phaseWorkflow.Name);
                    this._phaseWorkflowsByName.Add(workflow.Name, workflow);

                    foreach (var phase in phaseWorkflow.Phases)
                    {
                        Dictionary<string, object> parametersCollection = new Dictionary<string, object>();
                        parametersCollection.Add("WorkflowUniqueName", phase.WorkflowUniqueName);

                        // Error Pathing needs to be reworked in this segment: this is being fixed up and put into Phase.cs
                        foreach (var param in phase.Parameters)
                        {
                            Type paramType = Type.GetType("System." + param.Type, false, false);
                            if (paramType != null)
                            {
                                object convertedValue = TypeConverter.Convert(param.Value, paramType);

                                if (!parametersCollection.ContainsKey(param.Name))
                                {
                                    parametersCollection.Add(param.Name, convertedValue);
                                }
                                else
                                {
                                    Message.Trace(Severity.Error, Resources.ErrorDuplicatePhaseParameterSpecified, phase.WorkflowUniqueName, param.Name);
                                    return;
                                }
                            }
                            else
                            {
                                Message.Trace(Severity.Error, Resources.ErrorInvalidPhaseParameterType, phase.WorkflowUniqueName, param.Name);
                                return;
                            }
                        } // end foreach var param

                        workflow.AddPhase(phase.WorkflowUniqueName, this._phasePluginLoader.RetrievePhase(phase.Name, parametersCollection));
                    } // end foreach Phase

                    foreach (var irVector in phaseWorkflow.IRVectors)
                    {
                        workflow.AddIRFlowVector(irVector.SourceWorkflowUniqueName, irVector.SinkWorkflowUniqueName);
                    } // end foreach irVector inside of the phaseWorkflow
                }
                else
                {
                    Message.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowDuplicatename, phaseWorkflow.Name);
                }
            } // end foreach PhaseWorkflows
            
            if (!this._phaseWorkflowsByName.ContainsKey(this._defaultWorkflowName))
            {
                Message.Trace(Severity.Error, Resources.ErrorInvalidWorkflowDefaultName, this._defaultWorkflowName);
                throw new Exception();
            }
        }
    }
}
