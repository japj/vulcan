using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableBaseNode : IEmittableAstNode
    {
        public AstTableBaseNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        #region IEmittableAstNode Members

        public bool Emit
        {
            get; set;
        }

        #endregion
    }
}

