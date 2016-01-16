using AstFramework.Model;

namespace VulcanEngine.IR.Ast
{
    public partial class AstAnnotationNode
    {
        public AstAnnotationNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
