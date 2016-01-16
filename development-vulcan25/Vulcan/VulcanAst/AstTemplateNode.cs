using AstFramework.Engine.Binding;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast
{
    public partial class AstTemplateNode : ITemplate
    {
        public UnboundReferences UnboundReferences { get; private set; }

        protected AstTemplateNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
            UnboundReferences = new UnboundReferences();
        }
    }
}

