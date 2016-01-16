using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Dimension
{
    [AstSchemaTypeBindingAttribute("DimensionHierarchyLevelElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDimensionHierarchyLevelNode : AstDimensionNamedBaseNode
    {
        //TODO: Do we need to customize referenceablename on this class to include the DimensionHierarchy as well?

        #region Private Storage
        private AstAttributeNode _attribute;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("AttributeName", "http://tempuri.org/vulcan2.xsd")]
        public AstAttributeNode Attribute
        {
            get { return _attribute; }
            set { _attribute = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDimensionHierarchyLevelNode() : base() {  }
        #endregion   // Default Constructor

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            validationItems.AddRange(Attribute.Validate());

            return validationItems;
        }
        #endregion  // Validation
    }
}

