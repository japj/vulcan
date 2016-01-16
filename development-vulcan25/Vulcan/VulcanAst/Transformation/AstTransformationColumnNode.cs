using System.ComponentModel;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstTransformationColumnNode
    {
        public AstTransformationColumnNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        [BrowsableAttribute(false)]
        public AstTransformationNode ParentTransformation
        {
            get { return ParentItem as AstTransformationNode; }
        }
    }
}
