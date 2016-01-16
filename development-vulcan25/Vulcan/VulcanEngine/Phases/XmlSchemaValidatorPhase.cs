using System;
using System.Collections.ObjectModel;
using AstFramework;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.Properties;

namespace VulcanEngine.Phases
{
    [PhaseFriendlyName("XmlSchemaValidatorPhase")]
    public class XmlSchemaValidatorPhase : IPhase
    {
        private readonly string _workflowUniqueName;

        #region IPhase Members

        public string Name
        {
            get { return "XmlSchemaValidatorPhase"; }
        }
        
        public string WorkflowUniqueName
        {
            get { return _workflowUniqueName; }
        }

        public Type InputIRType
        {
            get { return typeof(XmlIR); }
        }

        public IIR Execute(Collection<IIR> predecessorIRs)
        {
            return Execute(IRUtility.MergeIRList(Name, predecessorIRs));
        }

        public IIR Execute(IIR predecessorIR)
        {
            var xmlIR = predecessorIR as XmlIR;
            if (xmlIR == null)
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorPhaseWorkflowIncorrectInputIRType, predecessorIR.GetType().ToString(), Name);
            }

            xmlIR.ValidateXDocuments();

            return xmlIR;
        }

        #endregion

        public XmlSchemaValidatorPhase(string workflowUniqueName)
        {
            _workflowUniqueName = workflowUniqueName;
        }
    }
}
