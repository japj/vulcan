using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableLookupBaseNode
    {
        public AstTableLookupBaseNode(IFrameworkItem parentAstNode)
            : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
