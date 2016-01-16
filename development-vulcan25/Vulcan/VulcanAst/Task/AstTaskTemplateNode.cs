using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstTaskTemplateNode
    {
        public AstTaskTemplateNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

