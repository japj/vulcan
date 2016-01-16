using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Dimension
{
    public class AstDimensionNamedBaseNode : AstNamedNode
    {
        #region Private Storage
        private AstDimensionNode _dimension;
        #endregion   // Private Storage

        #region Public Accessor Properties       
        public AstDimensionNode Dimension
        {
            get
            {
                if (this._dimension != null)
                {
                    return this._dimension;
                }
                AstNode currentNode = this;
                while (currentNode != null)
                {
                    AstDimensionNode tableNode = currentNode as AstDimensionNode;
                    if (tableNode != null)
                    {
                        this._dimension = tableNode;
                        return this._dimension;
                    }
                    currentNode = currentNode.ParentASTNode;
                }
                return null;
            }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDimensionNamedBaseNode() { }
        #endregion   // Default Constructor

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            return validationItems;
        }
        #endregion  // Validation
    }
}
