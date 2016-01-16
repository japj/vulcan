using AstFramework.Model;

namespace VulcanEngine.IR.Ast
{
    public partial class AstTemplateArgumentNode
    {
        public AstTemplateArgumentNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

