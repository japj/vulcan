using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Ssis2008Emitter.IR.Common;
using VulcanEngine.IR.Ast.Task;

namespace Ssis2008Emitter.IR.Tasks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class DataflowTask : Task
    {
        public DataflowTask(AstEtlRootNode astNode) : base(astNode)
        {
        }

        public override void Emit(SsisEmitterContext context)
        {
            base.Emit(context);
            foreach (PhysicalObject po in Children)
            {
                po.Initialize(context);
                po.Emit(context);
            }
        }

        public override string Moniker
        {
            get { return "STOCK:PipelineTask"; }
        }

        public override Executable DtsExecutable
        {
            get { return DtsTaskHost; }
        }

        public MainPipe MainPipe
        {
            get { return (MainPipe)DtsTaskHost.InnerObject; }
        }

        public IDTSComponentMetaData100 NewComponentMetadata()
        {
            return MainPipe.ComponentMetaDataCollection.New();
        }
    }
}
