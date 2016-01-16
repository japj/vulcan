using Ssis2008Emitter.IR.Framework;
using VulcanEngine.IR.Ast.Task;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public abstract class Task : Executable
    {
        public System.Data.IsolationLevel TaskIsolationLevel { get; set; }

        protected Task(AstTaskNode astNode)
            : base(astNode)
        {
            DelayValidation = astNode.DelayValidation;
            switch (astNode.IsolationLevel)
            {
                case IsolationLevel.Chaos:
                    TaskIsolationLevel = System.Data.IsolationLevel.Chaos;
                    break;
                case IsolationLevel.ReadCommitted:
                    TaskIsolationLevel = System.Data.IsolationLevel.ReadCommitted;
                    break;
                case IsolationLevel.ReadUncommitted:
                    TaskIsolationLevel = System.Data.IsolationLevel.ReadUncommitted;
                    break;
                case IsolationLevel.RepeatableRead:
                    TaskIsolationLevel = System.Data.IsolationLevel.RepeatableRead;
                    break;
                case IsolationLevel.Serializable:
                    TaskIsolationLevel = System.Data.IsolationLevel.Serializable;
                    break;
                case IsolationLevel.Snapshot:
                    TaskIsolationLevel = System.Data.IsolationLevel.Snapshot;
                    break;
                case IsolationLevel.Unspecified:
                    TaskIsolationLevel = System.Data.IsolationLevel.Unspecified;
                    break;
                default:
                    TaskIsolationLevel = System.Data.IsolationLevel.Serializable;
                    break;
            }
        }

        public bool DelayValidation { get; set; }

        public DTS.TaskHost DtsTaskHost { get; protected set; }

        public string Description { get; set; }

        public override DTS.EventsProvider DtsEventsProvider
        {
            get { return DtsTaskHost; }
        }

        public override void Initialize(Common.SsisEmitterContext context)
        {
            base.Initialize(context);
            DtsTaskHost = (DTS.TaskHost)context.ParentContainer.AppendExecutable(Moniker);
            DtsTaskHost.Name = Name;
            DtsTaskHost.Description = Description;
            DtsTaskHost.DelayValidation = DelayValidation;
            DtsTaskHost.IsolationLevel = TaskIsolationLevel;
        }

        public override void Emit(Common.SsisEmitterContext context)
        {
            context.ParentContainer.ProcessTaskBinding(this);
        }

        public override Microsoft.SqlServer.Dts.Runtime.IDTSPropertiesProvider PropertyProvider
        {
            get { return DtsTaskHost; }
        }
    }
}
