using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableIdentityNode
    {
        public AstTableIdentityNode(IFrameworkItem parentAstNode)
            : base(parentAstNode)
        {
            InitializeAstNode();
            base.Clustered = true;
        }
    }
}
