using AstFramework.Model;
using VulcanEngine.IR.Ast.Task;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableStaticSourceNode
    {
        public AstPackageNode LoweredPackage { get; set; }

        public AstTableStaticSourceNode(IFrameworkItem parentAstNode)
            : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
