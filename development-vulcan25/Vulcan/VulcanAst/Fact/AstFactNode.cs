using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Fact
{
    public partial class AstFactNode
    {
        public AstFactNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
