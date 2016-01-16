using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTablePrimaryKeyNode
    {
        public AstTablePrimaryKeyNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
            base.Clustered = true;
        }
    }
}
