using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstContainerTaskBaseNode
    {
        protected AstContainerTaskBaseNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

