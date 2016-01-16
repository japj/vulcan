using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using AstFramework;
using VulcanEngine.Common;
using VulcanEngine.Properties;

namespace VulcanEngine.Kernel
{
    public class PhaseWorkflowLoader
    {
        private string _defaultWorkflowName;
        private readonly PhasePluginLoader _phasePluginLoader;
        private readonly Dictionary<string, PhaseWorkflow> _phaseWorkflowsByName;
        private XmlSchemaSet _phaseWorkflowSchemaSet;

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
            _phasePluginLoader = new PhasePluginLoader();
            _phaseWorkflowsByName = new Dictionary<string, PhaseWorkflow>();

            LoadWorkflows();
        }

        private void LoadWorkflows()
        {
            var assembly = Assembly.GetExecutingAssembly();

            _phaseWorkflowSchemaSet = new XmlSchemaSet();
            _phaseWorkflowSchemaSet.Add(null, XmlReader.Create(assembly.GetManifestResourceStream("VulcanEngine.Content.xsd.VulcanPhaseWorkflows.xsd")));
            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = _phaseWorkflowSchemaSet };
            settings.ValidationEventHandler += Settings_ValidationEventHandler;

            XDocument doc = XDocument.Load(XmlReader.Create(assembly.GetManifestResourceStream("VulcanEngine.VulcanPhaseWorkflows.xml"), settings));
            
            // TODO: VSABELLA: REVIEW Remove hardcode schema for now, and load it from the document itself with XPathNavigator.GetNamespaceInScope.  
            XNamespace ns = "http://schemas.microsoft.com/detego/2007/07/07/VulcanPhaseWorkflows.xsd";

            _defaultWorkflowName = doc.Root.Attribute("DefaultWorkflow").Value;
            var phaseWorkflows = from pw in doc.Descendants(ns + "PhaseWorkflow")
                                 select new
                                 {
                                     Name = pw.Attribute("Name").Value,
                                     Phases = from phase in pw.Descendants(ns + "Phase")
                                               select new
                                               {
                                                   Name = phase.Attribute("PhaseName").Value,
                                                   WorkflowUniqueName = phase.Attribute("WorkflowUniqueName").Value,
                                                   Parameters = from param in phase.Descendants(ns + "Parameter")
                                                                 select new
                                                                 {
                                                                     Name = param.Attribute("Name").Value,
                                                                     Type = param.Attribute("Type").Value,
                                                                     Value = param.Value
                                                                 }
                                               },

                                     IRVectors = from irVector in pw.Descendants(ns + "IRFlowVector")
                                                  select new
                                                  {
                                                      SourceWorkflowUniqueName = irVector.Attribute("SourceWorkflowUniqueName").Value,
                                                      SinkWorkflowUniqueName = irVector.Attribute("SinkWorkflowUniqueName").Value
                                                  }
                                 };

            foreach (var phaseWorkflow in phaseWorkflows)
            {
                if (!_phaseWorkflowsByName.ContainsKey(phaseWorkflow.Name))
                {
                    var workflow = new PhaseWorkflow(phaseWorkflow.Name);
                    _phaseWorkflowsByName.Add(workflow.Name, workflow);

                    foreach (var phase in phaseWorkflow.Phases)
                    {
                        var parametersCollection = new Dictionary<string, object>();
                        parametersCollection.Add("workflowUniqueName", phase.WorkflowUniqueName);

                        // Error Pathing needs to be reworked in this segment: this is being fixed up and put into Phase.cs
                        foreach (var param in phase.Parameters)
                        {
                            Type paramType = Type.GetType("System." + param.Type, false, false);
                            if (paramType != null)
                            {
                                object convertedValue = param.Value; ////VulcanTypeConverter.Convert(param.Value, paramType);

                                if (!parametersCollection.ContainsKey(param.Name))
                                {
                                    parametersCollection.Add(param.Name, convertedValue);
                                }
                                else
                                {
                                    MessageEngine.Trace(Severity.Error, Resources.ErrorDuplicatePhaseParameterSpecified, phase.WorkflowUniqueName, param.Name);
                                    return;
                                }
                            }
                            else
                            {
                                MessageEngine.Trace(Severity.Error, Resources.ErrorInvalidPhaseParameterType, phase.WorkflowUniqueName, param.Name);
                                return;
                            }
                        } // end foreach var param

                        workflow.AddPhase(phase.WorkflowUniqueName, _phasePluginLoader.RetrievePhase(phase.Name, parametersCollection));
                    } // end foreach Phase

                    foreach (var vector in phaseWorkflow.IRVectors)
                    {
                        workflow.AddIRFlowVector(vector.SourceWorkflowUniqueName, vector.SinkWorkflowUniqueName);
                    } // end foreach irVector inside of the phaseWorkflow
                }
                else
                {
                    MessageEngine.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowDuplicatename, phaseWorkflow.Name);
                }
            } // end foreach PhaseWorkflows
            
            if (!_phaseWorkflowsByName.ContainsKey(_defaultWorkflowName))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorInvalidWorkflowDefaultName, _defaultWorkflowName);
            }
        }

        private void Settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            MessageEngine.Trace(Severity.Error, "PhaseWorkflowLoader: Schema Validation Error: " + e.Message);
        }
    }
}
