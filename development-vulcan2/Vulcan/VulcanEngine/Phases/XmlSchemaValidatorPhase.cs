using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;

using VulcanEngine.Properties;
using VulcanEngine.IR;
using VulcanEngine.Common;

namespace VulcanEngine.Phases
{
    [PhaseFriendlyName("XmlSchemaValidatorPhase")]
    public class XmlSchemaValidatorPhase : IPhase
    {
        string _WorkflowUniqueName;
        MessageEngine Message;

        #region IPhase Members

        public string Name
        {
            get { return "XmlSchemaValidatorPhase"; }
        }
        
        public string WorkflowUniqueName
        {
            get { return this._WorkflowUniqueName; }
        }

        public Type InputIRType
        {
            get { return typeof(XmlIR); }
        }

        public VulcanEngine.IR.IIR Execute(List<VulcanEngine.IR.IIR> PredecessorIRs)
        {
            return this.Execute(IRUtility.MergeIRList(this.Name, PredecessorIRs));
        }

        public VulcanEngine.IR.IIR Execute(VulcanEngine.IR.IIR PredecessorIR)
        {
            XmlIR xmlIR = PredecessorIR as XmlIR;
            if (xmlIR == null)
            {
                Message.Trace(Severity.Error, Resources.ErrorPhaseWorkflowIncorrectInputIRType, PredecessorIR.GetType().ToString(), this.Name);
            }

            xmlIR.ValidateXDocuments();

            return xmlIR;
        }

        #endregion

        public XmlSchemaValidatorPhase(string WorkflowUniqueName)
        {
            this._WorkflowUniqueName = WorkflowUniqueName;
            this.Message = MessageEngine.Create(WorkflowUniqueName);
        }
    }
}
