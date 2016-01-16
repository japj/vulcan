using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;
using VulcanEngine.IR.Ast.Transformation;
using VulcanEngine.Phases;

namespace VulcanLogging
{
    [PhaseFriendlyName("AstLoggingPhase")]
    public class AstLoggingPhase : IPhase
    {
        public string Name
        {
            get { return "AstLoggingPhase"; }
        }

        public string WorkflowUniqueName { get; protected set; }

        public Type InputIRType
        {
            get { return typeof(AstIR); }
        }

        public AstLoggingPhase(string workflowUniqueName)
        {
            WorkflowUniqueName = workflowUniqueName;
        }

        public IIR Execute(Collection<IIR> predecessorIRs)
        {
            return Execute(IRUtility.MergeIRList(Name, predecessorIRs));
        }

        public IIR Execute(IIR predecessorIR)
        {
            var ir = (AstIR)predecessorIR;
            VulcanLogging.ProcessPackages(ir.AstRootNode);
            return ir;
        }
    }
}
