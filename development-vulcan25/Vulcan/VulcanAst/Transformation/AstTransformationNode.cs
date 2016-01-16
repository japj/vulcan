using System.ComponentModel;
using AstFramework.Dataflow;
using AstFramework.Markup;
using AstFramework.Model;
using Vulcan.Utility.Collections;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstTransformationNode : IDataflowItem
    {
        // TODO: We should wire up change reporting on all of these.  Perhaps move into AstDesigner.
        [BrowsableAttribute(false)]        
        [AstMergeableProperty(MergeablePropertyType.Definition)]
        public AstDataflowOutputPathNode PreferredOutputPath { get; set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public VulcanCollection<AstTransformationColumnNode> DefinedColumns { get; private set; }

        [BrowsableAttribute(false)]
        [AstMergeablePropertyAttribute(MergeablePropertyType.Definition)]
        public VulcanCollection<AstDataflowOutputPathNode> StaticOutputPaths { get; private set; }

        protected AstTransformationNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
            DefinedColumns = new VulcanCollection<AstTransformationColumnNode>();
            StaticOutputPaths = new VulcanCollection<AstDataflowOutputPathNode>();
        }
    }
}
