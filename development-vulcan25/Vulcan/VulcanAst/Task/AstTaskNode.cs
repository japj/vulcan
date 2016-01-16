using System.ComponentModel;
using AstFramework.Dataflow;
using AstFramework.Markup;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstTaskNode : IDataflowItem
    {
        [BrowsableAttribute(false)]
        [AstMergeableProperty(MergeablePropertyType.Definition)]
        public AstTaskflowOutputPathNode OutputPath { get; private set; }

        protected AstTaskNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
            OutputPath = new AstTaskflowOutputPathNode(this) { Name = "Output" };
            OutputPath.DefineSymbol();
        }
    }
}
