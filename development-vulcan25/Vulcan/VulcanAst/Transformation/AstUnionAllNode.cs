using System.ComponentModel;
using AstFramework.Markup;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstUnionAllNode
    {
        // TODO: What is this actually called?
        private const string OutputSsisName = "Union All Output 1";

        [BrowsableAttribute(false)]
        [AstMergeableProperty(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode OutputPath { get; private set; }

        public AstUnionAllNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            OutputPath = new AstDataflowOutputPathNode(this) { Name = "Output", SsisName = OutputSsisName };

            OutputPath.DefineSymbol();

            PreferredOutputPath = OutputPath;

            StaticOutputPaths.Add(OutputPath);
        }
    }
}
