using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstEtlFragmentNode
    {
        public AstEtlFragmentNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
