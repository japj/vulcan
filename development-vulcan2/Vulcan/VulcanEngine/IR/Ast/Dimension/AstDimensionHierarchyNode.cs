using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Dimension
{
    [AstScopeBoundaryAttribute()]
    [AstSchemaTypeBindingAttribute("DimensionHierarchyElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDimensionHierarchyNode : AstDimensionNamedBaseNode
    {
        #region Private Storage
        private VulcanCollection<AstDimensionHierarchyLevelNode> _levels;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("Level", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstDimensionHierarchyLevelNode> Levels
        {
            get { return _levels; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDimensionHierarchyNode() : base()
        {
            this._levels = new VulcanCollection<AstDimensionHierarchyLevelNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Levels.Cast<AstNode>());
                return children;
            }
        }

        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            foreach (AstNode child in this.Children)
            {
                validationItems.AddRange(child.Validate());
            }

            return validationItems;
        }
        #endregion  // Validation
    }
}
