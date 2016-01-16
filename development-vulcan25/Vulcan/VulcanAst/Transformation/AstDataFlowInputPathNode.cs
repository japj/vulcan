using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstDataflowInputPathNode
    {
        public AstDataflowInputPathNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
