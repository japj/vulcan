using AstFramework.Model;

namespace VulcanEngine.IR.Ast
{
    public abstract partial class AstSecurableNode
    {
        protected AstSecurableNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
