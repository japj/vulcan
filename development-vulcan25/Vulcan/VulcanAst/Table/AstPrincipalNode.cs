using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstPrincipalNode : IEmittableAstNode
    {
        public AstPrincipalNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
