using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstSchemaNode : IEmittableAstNode
    {
        public AstSchemaNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
