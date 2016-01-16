using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Ssis2008Emitter.IR;
using Ssis2008Emitter.IR.Common;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Dimension;
using VulcanEngine.IR.Ast.Fact;
using VulcanEngine.Phases;
using Table = VulcanEngine.IR.Ast.Table;
using Task = VulcanEngine.IR.Ast.Task;

namespace Ssis2008Emitter.Phases.Lowering.Framework
{
    [PhaseFriendlyName("AstToPhysicalLoweringPhase")]
    public class AstToPhysicalLoweringPhase : IPhase
    {
        private string _workflowUniqueName;

        #region IPhase Members

        public string Name
        {
            get { return "XmlToAstParserPhase"; }
        }

        public string WorkflowUniqueName
        {
            get { return _workflowUniqueName; }
        }

        public Type InputIRType
        {
            get { return typeof(AstIR); }
        }

        public IIR Execute(Collection<IIR> predecessorIRs)
        {
            return Execute(IRUtility.MergeIRList(Name, predecessorIRs));
        }

        public IIR Execute(IIR predecessorIR)
        {
            AstIR astIR = predecessorIR as AstIR;
            if (astIR == null)
            {
                return null;
            }

            // Initialize Lowering Engine
            InitializeLoweringEngine();

            PhysicalIR physicalIR = new PhysicalIR(astIR);

            foreach (Task.AstPackageNode packageNode in astIR.AstRootNode.Packages)
            {
                if (packageNode.Emit)
                {
                    var root = physicalIR.InitializePackage(packageNode.Name);
                    PhysicalLoweringProcessor.Lower(packageNode, new TaskLoweringContext(root));
                }
            }

            return physicalIR;
        }
        #endregion

        #region Initialization

        private static bool _initialized;

        private static void InitializeLoweringEngine()
        {
            if (!_initialized)
            {
                _initialized = true;
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                foreach (Type t in currentAssembly.GetExportedTypes())
                {
                    foreach (MethodInfo mi in t.GetMethods())
                    {
                        object[] loweringAttributes = mi.GetCustomAttributes(typeof(LoweringAttribute), true);
                        foreach (LoweringAttribute la in loweringAttributes)
                        {
                            var handler = (AstLoweringHandler) Delegate.CreateDelegate(typeof(AstLoweringHandler), mi, true);
                            PhysicalLoweringProcessor.Register(la.AstNodeType, handler);
                        }
                    }
                }
            }
        }

        public AstToPhysicalLoweringPhase(string workflowUniqueName)
        {
            _workflowUniqueName = workflowUniqueName;
        }
        #endregion  // Initialization
    }
}
