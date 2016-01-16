using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableColumnTableReferenceBaseNode
    {
        protected AstTableColumnTableReferenceBaseNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

