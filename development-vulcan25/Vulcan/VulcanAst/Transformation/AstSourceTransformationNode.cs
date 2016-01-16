using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public abstract partial class AstSourceTransformationNode
    {
        protected AstSourceTransformationNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
