using System.ComponentModel;
using AstFramework.Markup;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstOleDBCommandNode
    {
        private const string OutputSsisName = "OLE DB Command Output";
        private const string ErrorSsisName = "OLE DB Command Error Output";

        [BrowsableAttribute(false)]
        [AstMergeableProperty(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode OutputPath { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode ErrorPath { get; private set; }

        public AstOleDBCommandNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            OutputPath = new AstDataflowOutputPathNode(this) { Name = "Output", SsisName = OutputSsisName };
            ErrorPath = new AstDataflowOutputPathNode(this) { Name = "Error", SsisName = ErrorSsisName };

            OutputPath.DefineSymbol();
            ErrorPath.DefineSymbol();

            PreferredOutputPath = OutputPath;

            StaticOutputPaths.Add(OutputPath);
            StaticOutputPaths.Add(ErrorPath);

            // TODO: Results?
            ////foreach (var column in Mappings)
            ////{
            ////    DefinedColumns.Add(new AstTransformationColumnNode(this) { ColumnName = column.DestinationName });
            ////}
        }
    }
}
