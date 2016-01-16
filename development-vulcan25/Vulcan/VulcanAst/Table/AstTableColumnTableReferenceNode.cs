using System.ComponentModel;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableColumnTableReferenceNode
    {
        public AstTableColumnTableReferenceNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        [Browsable(false)]
        public override bool IsComputed
        {
            // TableReference types are never computed
            get { return false; }
        }
    }
}

