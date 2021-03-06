using System.ComponentModel;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstMultipleColumnTableReferenceNode
    {
        public AstMultipleColumnTableReferenceNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        [BrowsableAttribute(false)]
        public override bool IsComputed
        {
            // TableReference types are never computed
            get { return false; }
        }
    }
}

