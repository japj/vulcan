using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AstFramework;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.Phases;
using VulcanEngine.Properties;

namespace VulcanEngine.Kernel
{
    // TODO: Consider porting this to use the generic Graph classes
    public class PhaseWorkflow : IDisposable
    {
        private List<PhaseExecutionHost> _phaseWorkflow;

        private Dictionary<string, PhaseExecutionHost> _phaseExecutionHostsByWorkflowUniqueName;

        public string Name { get; private set; }

        public PhaseWorkflow(string name)
        {
            Name = name;
            _phaseWorkflow = new List<PhaseExecutionHost>();
            _phaseExecutionHostsByWorkflowUniqueName = new Dictionary<string, PhaseExecutionHost>();
        }

        public PhaseExecutionHost AddPhase(string workflowUniqueName, IPhase phase)
        {
            if (_phaseExecutionHostsByWorkflowUniqueName.ContainsKey(workflowUniqueName))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowDuplicateUniquePhaseName, workflowUniqueName);
                return null;
            }

            var host = new PhaseExecutionHost(workflowUniqueName, phase);
            _phaseExecutionHostsByWorkflowUniqueName.Add(workflowUniqueName, host);
            _phaseWorkflow.Add(host);

            return host;
        }

        public void RemovePhase(string workflowUniqueName)
        {
            if (! _phaseExecutionHostsByWorkflowUniqueName.ContainsKey(workflowUniqueName))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorAttemptedRemoveWorkflowUniquePhaseNameDoesntExist, workflowUniqueName);
                return;
            }

            var host = _phaseExecutionHostsByWorkflowUniqueName[workflowUniqueName];
            _phaseWorkflow.Remove(host);
            _phaseExecutionHostsByWorkflowUniqueName.Remove(workflowUniqueName);

            return;
        }

        public void AddIRFlowVector(string sourceWorkflowUniqueName, string sinkWorkflowUniqueName)
        {
            if (!_phaseExecutionHostsByWorkflowUniqueName.ContainsKey(sourceWorkflowUniqueName))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorInvalidWorkflowPhaseSpecifier, sourceWorkflowUniqueName);
                return;
            }

            if (!_phaseExecutionHostsByWorkflowUniqueName.ContainsKey(sinkWorkflowUniqueName))
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorInvalidWorkflowPhaseSpecifier, sinkWorkflowUniqueName);
                return;
            }

            PhaseExecutionHost sourceHost = _phaseExecutionHostsByWorkflowUniqueName[sourceWorkflowUniqueName];
            PhaseExecutionHost sinkHost = _phaseExecutionHostsByWorkflowUniqueName[sinkWorkflowUniqueName];

            sourceHost.AddSuccessor(sinkHost);
            sinkHost.AddPredecessor(sourceHost);
        }

        private bool IsPhaseWorkflowGraphAcyclic
        {
            get
            {
                List<PhaseExecutionHost> rootHosts = _phaseWorkflow.FindAll(host => host.IsRootPhase);
                var visitedPhaseInProgress = new Dictionary<PhaseExecutionHost, bool>();

                if (_phaseWorkflow.Count > 0 && rootHosts.Count == 0)
                {
                    return false;
                }

                foreach (PhaseExecutionHost rootHost in rootHosts)
                {
                    var dfsHostStack = new Stack<PhaseExecutionHost>();
                    dfsHostStack.Push(rootHost);
                    while (dfsHostStack.Count > 0)
                    {
                        PhaseExecutionHost currentHost = dfsHostStack.Pop();

                        if (currentHost.IsLeafPhase)
                        {
                            visitedPhaseInProgress[currentHost] = false;
                        }
                        else if (!visitedPhaseInProgress.ContainsKey(currentHost))
                        {
                            visitedPhaseInProgress.Add(currentHost, true);
                            dfsHostStack.Push(currentHost);  // Pushing back ensures that it gets completed after children are completed.
                            foreach (PhaseExecutionHost successor in currentHost.Successors)
                            {
                                dfsHostStack.Push(successor);
                            }
                        }
                        else if (visitedPhaseInProgress[currentHost])
                        {
                            foreach (PhaseExecutionHost childHost in currentHost.Successors)
                            {
                                // REVIEW: Check order of execution rules on this.  can the indexer generate an exception?
                                if (!visitedPhaseInProgress.ContainsKey(childHost) || visitedPhaseInProgress[childHost])
                                {
                                    return false;
                                }
                            }

                            visitedPhaseInProgress[currentHost] = false;
                        }
                        //// else { No need to re-inspect completed subgraphs }
                    }
                }

                return true;
            }
        }

        // REVIEW: Root phases are special cased in a variety of locations.  Perhaps we can simplify.
        public void ExecutePhaseWorkflowGraph(IIR rootIR)
        {
            // Setup Root Phases
            List<PhaseExecutionHost> rootHosts = _phaseWorkflow.FindAll(host => host.IsRootPhase);
            foreach (PhaseExecutionHost rootHost in rootHosts)
            {
                rootHost.SupplyRootIR(rootIR);
            }

            // Check Graph For Cycles
            if (!IsPhaseWorkflowGraphAcyclic)
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorWorkflowCycleDetected, Name);
                return;
            }

            // Execute Graph
            var executionWaitHandles = new List<EventWaitHandle>();
            List<PhaseExecutionHost> notStarted = _phaseWorkflow.FindAll(host => !host.ExecutionStarted);
            while (notStarted.Count > 0)
            {
                List<PhaseExecutionHost> runnable = notStarted.FindAll(host => host.IsPredecessorIRFullySpecified);

                foreach (PhaseExecutionHost host in runnable)
                {
                    var executionWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, host.WorkflowUniqueName);
                    executionWaitHandles.Add(executionWaitHandle);

                    ThreadStart phaseThreadJob = delegate { host.Execute(executionWaitHandle); };
                    var phaseThread = new Thread(phaseThreadJob);

                    phaseThread.Start();
                }

                int index = WaitHandle.WaitAny(executionWaitHandles.ToArray());
                executionWaitHandles.RemoveAt(index);

                if (_phaseWorkflow.Any(host => host.FatalErrorOccurred))
                {
                    throw new InvalidOperationException("Fatal Error Occurred.");
                }

                notStarted = _phaseWorkflow.FindAll(host => !host.ExecutionStarted);
            }
        }

        public void Dispose()
        {
            // TODO: Do we need to do anything to our execution wait handles
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _phaseWorkflow = null;
                _phaseExecutionHostsByWorkflowUniqueName = null;
                Name = null;
            }
        }
    }
}
