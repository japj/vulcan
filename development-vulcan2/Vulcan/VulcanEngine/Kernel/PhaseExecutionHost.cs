using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

using VulcanEngine.Properties;
using VulcanEngine.Phases;
using VulcanEngine.IR;
using VulcanEngine.Common;

namespace VulcanEngine.Kernel
{
    public class PhaseExecutionHost
    {
        #region Private Storage
        string _WorkflowUniqueName;
        IPhase _HostedPhase;
        List<PhaseExecutionHost> _Predecessors;
        List<PhaseExecutionHost> _Successors;
        bool _ExecutionComplete;
        object _ExecutionCompleteLockObject = new object();
        bool _ExecutionStarted;
        object _ExecutionStartedLockObject = new object();

        object _ExecuteLockObject = new object();

        IIR _RootIR;
        Dictionary<PhaseExecutionHost, IIR> _PredecessorIRs;

        private MessageEngine Message;
        #endregion

        #region Accessor Properties
        public string WorkflowUniqueName
        {
            get { return _WorkflowUniqueName; }
        }

        public IPhase HostedPhase
        {
            get { return _HostedPhase; }
        }

        public ReadOnlyCollection<PhaseExecutionHost> Predecessors
        {
            get { return new ReadOnlyCollection<PhaseExecutionHost>(_Predecessors); }
        }

        public ReadOnlyCollection<PhaseExecutionHost> Successors
        {
            get { return new ReadOnlyCollection<PhaseExecutionHost>(_Successors); }
        }

        public int PredecessorCount
        {
            get { return this._Predecessors.Count; }
        }

        public int SuccessorCount
        {
            get { return this._Successors.Count; }
        }

        public bool IsRootPhase
        {
            get { return PredecessorCount == 0; }
        }

        public bool IsLeafPhase
        {
            get { return SuccessorCount == 0; }
        }

        public bool ExecutionStarted
        {
            get
            {
                lock (this._ExecutionStartedLockObject)
                {
                    return _ExecutionStarted;
                }
            }
            set 
            {
                lock (this._ExecutionStartedLockObject)
                {
                    _ExecutionStarted = value;
                }
            }
        }
        public bool ExecutionComplete
        {
            get
            {
                lock (this._ExecutionCompleteLockObject)
                {
                    return _ExecutionComplete;
                }
            }
            set
            {
                lock (this._ExecutionCompleteLockObject)
                {
                    _ExecutionComplete = value;
                }
            }
        }
        #endregion

        public PhaseExecutionHost(string WorkflowUniqueName, IPhase HostedPhase)
        {
            this._WorkflowUniqueName = WorkflowUniqueName;
            this._HostedPhase = HostedPhase;
            this._Predecessors = new List<PhaseExecutionHost>();
            this._Successors = new List<PhaseExecutionHost>();
            this._PredecessorIRs = new Dictionary<PhaseExecutionHost, IIR>();

            this.ExecutionStarted = false;
            this.ExecutionComplete = false;

            this.Message = MessageEngine.Create(String.Format("__PHASE EXECUTION HOST: {0}", WorkflowUniqueName));
        }

        public override bool Equals(object obj)
        {
            PhaseExecutionHost Host = obj as PhaseExecutionHost;
            return (Host != null && this._WorkflowUniqueName.Equals(Host._WorkflowUniqueName));
        }

        public override int GetHashCode()
        {
            return this._WorkflowUniqueName.GetHashCode();
        }

        public bool IsPredecessorIRFullySpecified
        {
            get
            {
                return (this._Predecessors.Count == this._PredecessorIRs.Count) && (!this.IsRootPhase || this._RootIR != null);
            }
        }

        public void SupplyRootIR(IIR RootIR)
        {
            this._RootIR = RootIR;
        }

        public void SupplyIRFromPredecessor(PhaseExecutionHost Predecessor, IIR IR)
        {
            if (!this._Predecessors.Contains(Predecessor))
            {
                Message.Trace(Severity.Error, Resources.ErrorInvalidWorkflowPhaseSpecifier, Predecessor._WorkflowUniqueName);
                return;
            }
            if (this._PredecessorIRs.ContainsKey(Predecessor))
            {
                Message.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowSupplyDuplicatePhaseIR, Predecessor._WorkflowUniqueName, this._WorkflowUniqueName);
                return;
            }
            this._PredecessorIRs.Add(Predecessor, IR);
        }

        public void AddPredecessor(PhaseExecutionHost Predecessor)
        {
            if (this._HostedPhase.Equals(Predecessor))
            {
                Message.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowSelfLoop, this._WorkflowUniqueName);
                return;
            }
            if (this._Predecessors.Contains(Predecessor))
            {
                Message.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowDuplicateIRFlowVector, Predecessor._WorkflowUniqueName, this._WorkflowUniqueName);
                return;
            }
            this._Predecessors.Add(Predecessor);
        }

        public void AddSuccessor(PhaseExecutionHost Sucessor)
        {
            if (this._HostedPhase.Equals(Sucessor))
            {
                Message.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowSelfLoop, this._WorkflowUniqueName);
                return;
            }
            if (this._Successors.Contains(Sucessor))
            {
                Message.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowDuplicateIRFlowVector, this._WorkflowUniqueName, Sucessor._WorkflowUniqueName);
                return;
            }
            this._Successors.Add(Sucessor);
        }

        public void Execute(EventWaitHandle ExecutionWaitHandle)
        {
            // REVIEW: This design may allow extra Executes to be invoked, and then block and ultimately terminate the superfluous thread.  It works, but can we make it cleaner?
            lock (this._ExecuteLockObject)
            {
                if (this.ExecutionStarted)
                {
                    return;
                }

                this.ExecutionStarted = true;

                IIR PhaseOutputIR;
                if (this.IsRootPhase)
                {
                    PhaseOutputIR = this._HostedPhase.Execute(this._RootIR);
                }
                else
                {
                    // REVIEW: Ignores which Input IR came from which predecessor IPhase for now.  Is this OK?
                    PhaseOutputIR = this._HostedPhase.Execute(new List<IIR>(this._PredecessorIRs.Values));
                }

                foreach (PhaseExecutionHost ChildHost in this._Successors)
                {
                    // TODO: Do we need to Clone here?
                    ChildHost.SupplyIRFromPredecessor(this, (IIR)PhaseOutputIR);
                }
                this.ExecutionComplete = true;
                ExecutionWaitHandle.Set();
            }
        }
    }
}
