using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstMultipleColumnTableReferenceBaseNode
    {
        protected AstMultipleColumnTableReferenceBaseNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

