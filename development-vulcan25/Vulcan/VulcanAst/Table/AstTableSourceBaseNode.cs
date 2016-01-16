using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableSourceBaseNode
    {
        protected AstTableSourceBaseNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
