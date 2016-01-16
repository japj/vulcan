using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.Phases.Lowering.Framework
{
    public abstract class LoweringContext
    {
        public PhysicalObject ParentObject { get; private set; }

        protected LoweringContext(PhysicalObject parentObject)
        {
            ParentObject = parentObject;
        }
    }
}
