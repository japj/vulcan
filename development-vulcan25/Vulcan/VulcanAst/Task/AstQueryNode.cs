using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstQueryNode
    {
        public AstQueryNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

