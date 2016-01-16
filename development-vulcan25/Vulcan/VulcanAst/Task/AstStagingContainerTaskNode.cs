using System.ComponentModel;
using AstFramework.Model;
using VulcanEngine.IR.Ast.Table;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstStagingContainerTaskNode
    {
        public AstStagingContainerTaskNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
