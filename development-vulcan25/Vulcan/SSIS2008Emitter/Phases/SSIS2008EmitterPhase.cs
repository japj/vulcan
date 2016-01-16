using System;
using System.Collections.ObjectModel;
using Ssis2008Emitter.IR;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using VulcanEngine.Common;
using VulcanEngine.Phases;

namespace Ssis2008Emitter
{
    [PhaseFriendlyNameAttribute("Ssis2008EmitterPhase")]
    public class Ssis2008EmitterPhase : IPhase
    {
        private readonly string _workflowUniqueName;

        public Ssis2008EmitterPhase(string workflowUniqueName)
        {
            _workflowUniqueName = workflowUniqueName;
        }

        #region IPhase Members

        public string Name
        {
            get { return "Ssis2008EmitterPhase"; }
        }

        public string WorkflowUniqueName
        {
            get { return _workflowUniqueName; }
        }

        public Type InputIRType
        {
            get { return typeof(PhysicalIR); }
        }

        public VulcanEngine.IR.IIR Execute(Collection<VulcanEngine.IR.IIR> predecessorIRs)
        {
            return Execute(IRUtility.MergeIRList(Name, predecessorIRs));
        }

        public VulcanEngine.IR.IIR Execute(VulcanEngine.IR.IIR predecessorIR)
        {
            var physicalIR = predecessorIR as PhysicalIR;
            if (physicalIR != null)
            {
                int emittablesCount = 0;
                foreach (PhysicalObject root in physicalIR.EmittableNodes)
                {
                    emittablesCount += root.Count;
                    foreach (PhysicalObject emittable in root.Children)
                    {
                        var package = emittable as Package;
                        if (package != null)
                        {
                            package.ClearProjectDirectory();
                        }
                    }
                }

                // TODO: Progress bar is whacky.
                int emittablesProcessed = 0;
                var context = new SsisEmitterContext();
                foreach (PhysicalObject root in physicalIR.EmittableNodes)
                {
                    MessageEngine.UpdateProgress(emittablesProcessed / (double)emittablesCount);
                    foreach (ISsisEmitter emittable in root.Children)
                    {
                        emittable.Initialize(context);
                        emittable.Emit(context);
                    }

                    emittablesProcessed += root.Count;
                    MessageEngine.UpdateProgress(emittablesProcessed / (double)emittablesCount);
                }
            }

            return null;
        }
        #endregion
    }
}