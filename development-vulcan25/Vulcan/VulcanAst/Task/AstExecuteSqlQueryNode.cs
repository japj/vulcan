using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstExecuteSqlQueryNode
    {
        public AstExecuteSqlQueryNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

