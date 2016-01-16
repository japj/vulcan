using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstEtlFragmentReferenceNode
    {
        public AstEtlFragmentReferenceNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            foreach (var column in Outputs)
            {
                DefinedColumns.Add(new AstTransformationColumnNode(this) { ColumnName = column.DestinationPathColumnName });
            }
        }
    }
}
