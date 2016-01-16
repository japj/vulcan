using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstDataflowColumnMappingNode
    {
        public AstDataflowColumnMappingNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
