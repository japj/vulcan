using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstVariableNode
    {
        public AstVariableNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            this.IsSystemVariable = false;
            InitializeAstNode();
        }

        public bool IsSystemVariable { get; set; }
    }
}
