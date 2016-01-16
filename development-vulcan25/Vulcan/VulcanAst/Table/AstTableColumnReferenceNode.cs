using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableColumnReferenceNode
    {
        protected AstTableColumnReferenceNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

