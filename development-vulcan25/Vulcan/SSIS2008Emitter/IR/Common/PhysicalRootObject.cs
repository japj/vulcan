namespace Ssis2008Emitter.IR.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class PhysicalRootObject : PhysicalObject
    {
        public override void Initialize(SsisEmitterContext context)
        {
        }

        public override void Emit(SsisEmitterContext context)
        {
        }

        public PhysicalRootObject(string name)
            : base(name)
        {
        }
    }
}
