using AstFramework.Model;
using System.Xml.Linq;
using Vulcan.Utility.Xml;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstRowCountNode
    {
        public AstRowCountNode(IFrameworkItem parentItem) : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

