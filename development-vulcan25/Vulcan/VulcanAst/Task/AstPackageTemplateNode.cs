using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstPackageTemplateNode
    {
        public AstPackageTemplateNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

