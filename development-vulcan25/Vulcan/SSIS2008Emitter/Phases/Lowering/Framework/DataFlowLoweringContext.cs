using Ssis2008Emitter.IR.Tasks;

namespace Ssis2008Emitter.Phases.Lowering.Framework
{
    public class DataflowLoweringContext : LoweringContext
    {
        public DataflowTask Dataflow { get; private set; }

        public DataflowLoweringContext(DataflowTask dataflow) : base(dataflow)
        {
            Dataflow = dataflow;
        }
    }
}
