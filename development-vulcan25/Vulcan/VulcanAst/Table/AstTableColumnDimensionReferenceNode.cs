using System.ComponentModel;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableColumnDimensionReferenceNode
    {
        public AstTableColumnDimensionReferenceNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        [Browsable(false)]
        public override bool IsComputed
        {
            // DimensionReference types are never computed
            get { return false; }
        }
    }
}

