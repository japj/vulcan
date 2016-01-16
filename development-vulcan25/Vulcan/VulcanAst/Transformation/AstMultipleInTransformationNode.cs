using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public abstract partial class AstMultipleInTransformationNode
    {
        protected AstMultipleInTransformationNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
