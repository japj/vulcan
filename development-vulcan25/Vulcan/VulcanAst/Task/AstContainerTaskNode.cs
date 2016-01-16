using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstContainerTaskNode
    {
        public AstContainerTaskNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
