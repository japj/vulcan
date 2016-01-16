using System.ComponentModel;
using AstFramework.Markup;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstSlowlyChangingDimensionNode
    {
        private const string UnchangedSsisName = "Unchanged Output";
        private const string NewSsisName = "New Output";
        private const string FixedAttributeSsisName = "Fixed Attribute Output";
        private const string ChangingAttributeSsisName = "Changing Attribute Updates Output";
        private const string HistoricalAttributeSsisName = "Historical Attribute Inserts Output";
        private const string InferredMemberSsisName = "Inferred Member Updates Output";

        [BrowsableAttribute(false)]
        [AstMergeableProperty(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode UnchangedPath { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode NewPath { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode FixedAttributePath { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode ChangingAttributePath { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode HistoricalAttributePath { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode InferredMemberPath { get; private set; }

        public AstSlowlyChangingDimensionNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            UnchangedPath = new AstDataflowOutputPathNode(this) { Name = "Unchanged", SsisName = UnchangedSsisName };
            NewPath = new AstDataflowOutputPathNode(this) { Name = "New", SsisName = NewSsisName };
            FixedAttributePath = new AstDataflowOutputPathNode(this) { Name = "FixedAttribute", SsisName = FixedAttributeSsisName };
            ChangingAttributePath = new AstDataflowOutputPathNode(this) { Name = "ChangingAttribute", SsisName = ChangingAttributeSsisName };
            HistoricalAttributePath = new AstDataflowOutputPathNode(this) { Name = "HistoricalAttribute", SsisName = HistoricalAttributeSsisName };
            InferredMemberPath = new AstDataflowOutputPathNode(this) { Name = "InferredMember", SsisName = InferredMemberSsisName };

            UnchangedPath.DefineSymbol();
            NewPath.DefineSymbol();
            FixedAttributePath.DefineSymbol();
            ChangingAttributePath.DefineSymbol();
            HistoricalAttributePath.DefineSymbol();
            InferredMemberPath.DefineSymbol();

            StaticOutputPaths.Add(UnchangedPath);
            StaticOutputPaths.Add(NewPath);
            StaticOutputPaths.Add(FixedAttributePath);
            StaticOutputPaths.Add(ChangingAttributePath);
            StaticOutputPaths.Add(HistoricalAttributePath);
            StaticOutputPaths.Add(InferredMemberPath);
        }
    }
}
