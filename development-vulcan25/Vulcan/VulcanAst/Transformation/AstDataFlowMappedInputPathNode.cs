using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstDataflowMappedInputPathNode
    {
        public AstDataflowMappedInputPathNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
