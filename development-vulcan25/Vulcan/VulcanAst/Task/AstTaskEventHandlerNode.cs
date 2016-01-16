using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstTaskEventHandlerNode
    {
        public AstTaskEventHandlerNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

