using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using AstFramework;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.Phases;
using VulcanEngine.Properties;

namespace VulcanEngine.Kernel
{
    public class PhaseExecutionHost
    {
        #region Private Storage
        private readonly string _workflowUniqueName;

        private readonly IPhase _hostedPhase;

        private readonly List<PhaseExecutionHost> _predecessors;

        private readonly List<PhaseExecutionHost> _successors;

        private bool _executionComplete;

        private readonly object _executionCompleteLockObject = new object();

        private bool _executionStarted;

        private bool _fatalErrorOccurred;

        private readonly object _executionStartedLockObject = new object();

        private readonly object _executeLockObject = new object();

        private IIR _rootIR;

        private readonly Dictionary<PhaseExecutionHost, IIR> _predecessorIRs;
        #endregion

        #region Accessor Properties
        public string WorkflowUniqueName
        {
            get { return _workflowUniqueName; }
        }

        public IPhase HostedPhase
        {
            get { return _hostedPhase; }
        }

        public ReadOnlyCollection<PhaseExecutionHost> Predecessors
        {
            get { return new ReadOnlyCollection<PhaseExecutionHost>(_predecessors); }
        }

        public ReadOnlyCollection<PhaseExecutionHost> Successors
        {
            get { return new ReadOnlyCollection<PhaseExecutionHost>(_successors); }
        }

        public int PredecessorCount
        {
            get { return _predecessors.Count; }
        }

        public int SuccessorCount
        {
            get { return _successors.Count; }
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
                lock (_executionStartedLockObject)
                {
                    return _executionStarted;
                }
            }

            set 
            {
                lock (_executionStartedLockObject)
                {
                    _executionStarted = value;
                }
            }
        }

        public bool ExecutionComplete
        {
            get
            {
                lock (_executionCompleteLockObject)
                {
                    return _executionComplete;
                }
            }

            set
            {
                lock (_executionCompleteLockObject)
                {
                    _executionComplete = value;
                }
            }
        }

        public bool FatalErrorOccurred
        {
            get
            {
                lock (_executionCompleteLockObject)
                {
                    return _fatalErrorOccurred;
                }
            }

            set
            {
                lock (_executionCompleteLockObject)
                {
                    _fatalErrorOccurred = value;
                }
            }
        }
        #endregion

        public PhaseExecutionHost(string workflowUniqueName, IPhase hostedPhase)
        {
            _workflowUniqueName = workflowUniqueName;
            _hostedPhase = hostedPhase;
            _predecessors = new List<PhaseExecutionHost>();
            _successors = new List<PhaseExecutionHost>();
            _predecessorIRs = new Dictionary<PhaseExecutionHost, IIR>();

            ExecutionStarted = false;
            ExecutionComplete = false;
        }

        public override bool Equals(object obj)
        {
            var host = obj as PhaseExecutionHost;
            return host != null && _workflowUniqueName.Equals(host._workflowUniqueName);
        }

        public override int GetHashCode()
        {
            return _workflowUniqueName.GetHashCode();
        }

        public bool IsPredecessorIRFullySpecified
        {
            get
            {
                return (_predecessors.Count == _predecessorIRs.Count) && (!IsRootPhase || _rootIR != null);
            }
        }

        public void SupplyRootIR(IIR rootIR)
        {
            _rootIR = rootIR;
        }

        public void SupplyIRFromPredecessor(PhaseExecutionHost predecessor, IIR ir)
        {
            if (!_predecessors.Contains(predecessor))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorInvalidWorkflowPhaseSpecifier, predecessor._workflowUniqueName);
                return;
            }

            if (_predecessorIRs.ContainsKey(predecessor))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowSupplyDuplicatePhaseIR, predecessor._workflowUniqueName, _workflowUniqueName);
                return;
            }

            _predecessorIRs.Add(predecessor, ir);
        }

        public void AddPredecessor(PhaseExecutionHost predecessor)
        {
            if (_hostedPhase.Equals(predecessor))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowSelfLoop, _workflowUniqueName);
                return;
            }

            if (_predecessors.Contains(predecessor))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowDuplicateIRFlowVector, predecessor._workflowUniqueName, _workflowUniqueName);
                return;
            }

            _predecessors.Add(predecessor);
        }

        public void AddSuccessor(PhaseExecutionHost successor)
        {
            if (_hostedPhase.Equals(successor))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowSelfLoop, _workflowUniqueName);
                return;
            }

            if (_successors.Contains(successor))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowDuplicateIRFlowVector, _workflowUniqueName, successor._workflowUniqueName);
                return;
            }

            _successors.Add(successor);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is the main exception swallower for the phase thread.")]
        public void Execute(EventWaitHandle executionWaitHandle)
        {
            try
            {
                // REVIEW: This design may allow extra Executes to be invoked, and then block and ultimately terminate the superfluous thread.  It works, but can we make it cleaner?
                lock (_executeLockObject)
                {
                    if (ExecutionStarted)
                    {
                        return;
                    }

                    ExecutionStarted = true;

                    IIR phaseOutputIR;
                    if (IsRootPhase)
                    {
                        phaseOutputIR = _hostedPhase.Execute(_rootIR);
                    }
                    else
                    {
                        // REVIEW: Ignores which Input IR came from which predecessor IPhase for now.  Is this OK?
                        phaseOutputIR = _hostedPhase.Execute(new Collection<IIR>(_predecessorIRs.Values.ToList()));
                    }

                    foreach (PhaseExecutionHost childHost in _successors)
                    {
                        // TODO: Do we need to Clone here?
                        childHost.SupplyIRFromPredecessor(this, phaseOutputIR);
                    }

                    ExecutionComplete = true;
                    executionWaitHandle.Set();
                }
            }
            catch (Exception)
            {
                FatalErrorOccurred = true;
                executionWaitHandle.Set();
            }
        }
    }
}
