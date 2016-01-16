using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstDestinationNode
    {
        public AstDestinationNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
