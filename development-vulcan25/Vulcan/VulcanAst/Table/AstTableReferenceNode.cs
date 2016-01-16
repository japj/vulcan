using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableReferenceNode
    {
        public AstTableReferenceNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
