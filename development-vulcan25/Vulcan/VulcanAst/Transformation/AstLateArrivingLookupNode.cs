using System.ComponentModel;
using AstFramework.Markup;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstLateArrivingLookupNode
    {
        // TODO: This is totally borken
        private const string OutputSsisName = "Derived Column Output";

        [BrowsableAttribute(false)]
        [AstMergeableProperty(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode OutputPath { get; private set; }

        public AstLateArrivingLookupNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            OutputPath = new AstDataflowOutputPathNode(this) { Name = "Output", SsisName = OutputSsisName };

            OutputPath.DefineSymbol();

            PreferredOutputPath = OutputPath;

            StaticOutputPaths.Add(OutputPath);

            foreach (var column in Outputs)
            {
                DefinedColumns.Add(new AstTransformationColumnNode(this) { ColumnName = column.LocalColumnName });
            }
        }
    }
}
