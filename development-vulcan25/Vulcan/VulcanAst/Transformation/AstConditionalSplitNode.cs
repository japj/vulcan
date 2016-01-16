using System.ComponentModel;
using AstFramework.Markup;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstConditionalSplitNode
    {
        ////private const string DefaultSsisName = "Conditional Split Default Output";
        private const string ErrorSsisName = "Conditional Split Error Output";

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode ErrorPath { get; private set; }

        public AstConditionalSplitNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            ErrorPath = new AstDataflowOutputPathNode(this) { Name = "Error", SsisName = ErrorSsisName };

            ErrorPath.DefineSymbol();

            DefaultOutputPath = new AstDataflowOutputPathNode(this) { Name = "DefaultOutput", SsisName = "DefaultOutput" };

            PreferredOutputPath = DefaultOutputPath;

            StaticOutputPaths.Add(ErrorPath);
        }
    }
}
