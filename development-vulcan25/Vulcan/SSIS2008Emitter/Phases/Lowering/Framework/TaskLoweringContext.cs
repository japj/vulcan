using System;
using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.Phases.Lowering.Framework
{
    public class TaskLoweringContext : LoweringContext
    {
        public TaskLoweringContext(PhysicalObject parentObject)
            : base(parentObject)
        {
        }
    }
}
