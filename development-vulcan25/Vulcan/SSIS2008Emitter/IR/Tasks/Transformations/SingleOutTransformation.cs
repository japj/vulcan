using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public abstract class SingleOutTransformation : Transformation
    {
        protected SingleOutTransformation(LoweringContext context, AstTransformationNode astTransformationNode) : base(context, astTransformationNode)
        {
        }

        public IDTSOutput100 OutputPath
        {
            get { return Component.OutputCollection[0]; }
        }
    }   
}
