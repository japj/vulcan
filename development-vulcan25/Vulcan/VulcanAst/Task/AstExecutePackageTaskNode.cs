using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstExecutePackageTaskNode
    {
        public AstExecutePackageTaskNode(IFrameworkItem parentAstNode)
            : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
