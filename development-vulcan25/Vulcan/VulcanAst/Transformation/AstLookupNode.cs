using System.ComponentModel;
using AstFramework.Markup;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstLookupNode
    {
        private const string MatchSsisName = "Lookup Match Output";
        private const string NoMatchSsisName = "Lookup No Match Output";
        private const string ErrorSsisName = "Lookup Error Output";

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode MatchPath { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeableProperty(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode NoMatchPath { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode ErrorPath { get; private set; }

        [BrowsableAttribute(false)]
        public bool EnableNoMatchOutputPath
        {
            get { return NoMatchPath != null && NoMatchPath.References.Count > 0; }
        }

        public AstLookupNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            MatchPath = new AstDataflowOutputPathNode(this) { Name = "Match", SsisName = MatchSsisName };
            NoMatchPath = new AstDataflowOutputPathNode(this) { Name = "NoMatch", SsisName = NoMatchSsisName };
            ErrorPath = new AstDataflowOutputPathNode(this) { Name = "Error", SsisName = ErrorSsisName };

            MatchPath.DefineSymbol();
            NoMatchPath.DefineSymbol();
            ErrorPath.DefineSymbol();

            PreferredOutputPath = MatchPath;

            StaticOutputPaths.Add(MatchPath);
            StaticOutputPaths.Add(NoMatchPath);
            StaticOutputPaths.Add(ErrorPath);

            foreach (var column in Outputs)
            {
                DefinedColumns.Add(new AstTransformationColumnNode(this) { ColumnName = column.LocalColumnName });
            }
        }
    }
}
