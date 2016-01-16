using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstMergeTaskNode
    {
        public AstMergeTaskNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
