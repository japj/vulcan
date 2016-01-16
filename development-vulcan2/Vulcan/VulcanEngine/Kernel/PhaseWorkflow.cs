using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using VulcanEngine.Phases;
using VulcanEngine.IR;
using VulcanEngine.Common;
using VulcanEngine.Properties;

namespace VulcanEngine.Kernel
{
    public class PhaseWorkflow
    {
        List<PhaseExecutionHost> _PhaseWorkflow;
        Dictionary<string, PhaseExecutionHost> _PhaseExecutionHostsByWorkflowUniqueName;
        Dictionary<string, bool> _PhaseExecutionStatusByWorkflowUniqueName;
        string _Name;

        private MessageEngine Message;

        public string Name
        {
            get { return _Name; }
        }

        public PhaseWorkflow(string Name)
        {
            this._Name = Name;
            this._PhaseWorkflow = new List<PhaseExecutionHost>();
            this._PhaseExecutionHostsByWorkflowUniqueName = new Dictionary<string, PhaseExecutionHost>();
            this._PhaseExecutionStatusByWorkflowUniqueName = new Dictionary<string, bool>();

            this.Message = MessageEngine.Create(String.Format("__PHASE WORKFLOW: {0}", Name));
        }

        public PhaseExecutionHost AddPhase(string WorkflowUniqueName, IPhase Phase)
        {
            if (this._PhaseExecutionHostsByWorkflowUniqueName.ContainsKey(WorkflowUniqueName))
            {
                Message.Trace(Severity.Error, Resources.ErrorAttemptedWorkflowDuplicateUniquePhaseName, WorkflowUniqueName);
                return null;
            }
            PhaseExecutionHost Host = new PhaseExecutionHost(WorkflowUniqueName, Phase);
            this._PhaseExecutionHostsByWorkflowUniqueName.Add(WorkflowUniqueName, Host);
            this._PhaseWorkflow.Add(Host);

            return Host;
        }

        public void AddIRFlowVector(string SourceWorkflowUniqueName, string SinkWorkflowUniqueName)
        {
            if (!this._PhaseExecutionHostsByWorkflowUniqueName.ContainsKey(SourceWorkflowUniqueName))
            {
                Message.Trace(Severity.Error, Resources.ErrorInvalidWorkflowPhaseSpecifier, SourceWorkflowUniqueName);
                return;
            }
            if (!this._PhaseExecutionHostsByWorkflowUniqueName.ContainsKey(SinkWorkflowUniqueName))
            {
                Message.Trace(Severity.Error, Resources.ErrorInvalidWorkflowPhaseSpecifier, SinkWorkflowUniqueName);
                return;
            }
            PhaseExecutionHost SourceHost = this._PhaseExecutionHostsByWorkflowUniqueName[SourceWorkflowUniqueName];
            PhaseExecutionHost SinkHost = this._PhaseExecutionHostsByWorkflowUniqueName[SinkWorkflowUniqueName];

            SourceHost.AddSuccessor(SinkHost);
            SinkHost.AddPredecessor(SourceHost);
        }


        bool IsPhaseWorkflowGraphAcyclic
        {
            get
            {
                List<PhaseExecutionHost> RootHosts = this._PhaseWorkflow.FindAll(delegate(PhaseExecutionHost Host) { return Host.IsRootPhase; });
                Dictionary<PhaseExecutionHost, bool> VisitedPhaseInProgress = new Dictionary<PhaseExecutionHost, bool>();

                if (this._PhaseWorkflow.Count > 0 && RootHosts.Count == 0)
                {
                    return false;
                }

                foreach (PhaseExecutionHost RootHost in RootHosts)
                {
                    Stack<PhaseExecutionHost> DFSHostStack = new Stack<PhaseExecutionHost>();
                    DFSHostStack.Push(RootHost);
                    while (DFSHostStack.Count > 0)
                    {
                        PhaseExecutionHost CurrentHost = DFSHostStack.Pop();

                        if (CurrentHost.IsLeafPhase)
                        {
                            VisitedPhaseInProgress[CurrentHost] = false;
                        }
                        else if (!VisitedPhaseInProgress.ContainsKey(CurrentHost))
                        {
                            VisitedPhaseInProgress.Add(CurrentHost, true);
                            DFSHostStack.Push(CurrentHost);  // Pushing back ensures that it gets completed after children are completed.
                            foreach (PhaseExecutionHost Successor in CurrentHost.Successors)
                            {
                                DFSHostStack.Push(Successor);
                            }
                        }
                        else if (VisitedPhaseInProgress[CurrentHost])
                        {
                            foreach (PhaseExecutionHost ChildHost in CurrentHost.Successors)
                            {
                                // REVIEW: Check order of execution rules on this.  can the indexer generate an exception?
                                if (!VisitedPhaseInProgress.ContainsKey(ChildHost) || VisitedPhaseInProgress[ChildHost])
                                {
                                    return false;
                                }
                            }
                            VisitedPhaseInProgress[CurrentHost] = false;
                        }
                        // else { No need to re-inspect completed subgraphs }
                    }
                }
                return true;
            }
        }

        // REVIEW: Root phases are special cased in a variety of locations.  Perhaps we can simplify.
        public void ExecutePhaseWorkflowGraph(IIR RootIR)
        {
            // Setup Root Phases
            List<PhaseExecutionHost> RootHosts = this._PhaseWorkflow.FindAll(delegate(PhaseExecutionHost Host) { return Host.IsRootPhase; });
            foreach (PhaseExecutionHost RootHost in RootHosts)
            {
                RootHost.SupplyRootIR(RootIR);
            }


            // Check Graph For Cycles
            if (!this.IsPhaseWorkflowGraphAcyclic)
            {
                Message.Trace(Severity.Error, Resources.ErrorWorkflowCycleDetected, this.Name);
                return;
            }


            // Execute Graph
            List<EventWaitHandle> ExecutionWaitHandles = new List<EventWaitHandle>();
            List<PhaseExecutionHost> NotStarted = this._PhaseWorkflow.FindAll(delegate(PhaseExecutionHost Host) { return !Host.ExecutionStarted; });
            while (NotStarted.Count > 0)
            {
                List<PhaseExecutionHost> Runnable = NotStarted.FindAll(delegate(PhaseExecutionHost Host) { return Host.IsPredecessorIRFullySpecified; });

                // TODO: Should we use the ThreadPool here?
                foreach (PhaseExecutionHost Host in Runnable)
                {
                    EventWaitHandle ExecutionWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, Host.WorkflowUniqueName);
                    ExecutionWaitHandles.Add(ExecutionWaitHandle);

                    ThreadStart PhaseThreadJob = new ThreadStart(delegate() { Host.Execute(ExecutionWaitHandle); });
                    Thread PhaseThread = new Thread(PhaseThreadJob);

                    PhaseThread.Start();
                }

                int index = EventWaitHandle.WaitAny(ExecutionWaitHandles.ToArray());
                ExecutionWaitHandles.RemoveAt(index);

                NotStarted = this._PhaseWorkflow.FindAll(delegate(PhaseExecutionHost Host) { return !Host.ExecutionStarted; });
            }
        }
    }
}
