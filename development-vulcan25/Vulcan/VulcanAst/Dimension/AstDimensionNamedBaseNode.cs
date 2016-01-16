using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Dimension
{
    public partial class AstDimensionNamedBaseNode
    {
        public AstDimensionNamedBaseNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        // TODO: Can these ever transfer between dimensions?  If so, the cached dimension copy can be outdated
        private AstDimensionNode _dimension;

        public AstDimensionNode Dimension
        {
            get
            {
                if (_dimension != null)
                {
                    return _dimension;
                }

                IFrameworkItem currentNode = this;
                while (currentNode != null)
                {
                    AstDimensionNode tableNode = currentNode as AstDimensionNode;
                    if (tableNode != null)
                    {
                        _dimension = tableNode;
                        return _dimension;
                    }

                    currentNode = currentNode.ParentItem;
                }

                return null;
            }
        }
    }
}
