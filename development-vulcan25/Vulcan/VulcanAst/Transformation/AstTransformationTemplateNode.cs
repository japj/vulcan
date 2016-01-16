using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstTransformationTemplateNode
    {
        public AstTransformationTemplateNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

