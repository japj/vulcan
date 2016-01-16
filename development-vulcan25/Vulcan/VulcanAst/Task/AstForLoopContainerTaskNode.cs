using AstFramework.Model;
using Vulcan.Utility.Xml;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstForLoopContainerTaskNode
    {
        public AstForLoopContainerTaskNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

