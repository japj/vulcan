using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OutSource", Justification = "The parser is confusing OutSource for Outsource.")]
    public partial class AstSingleOutSourceNode
    {
        public AstSingleOutSourceNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
