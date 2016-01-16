using AstFramework.Model;

namespace VulcanEngine.IR.Ast
{
    public abstract partial class AstScopeBoundaryNode
    {
        ////[System.ComponentModel.BrowsableAttribute(false)]
        ////public SymbolTable SymbolTable { get; private set; }

        protected AstScopeBoundaryNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            ////SymbolTable = new SymbolTable(this);
            InitializeAstNode();
        }
    }
}
