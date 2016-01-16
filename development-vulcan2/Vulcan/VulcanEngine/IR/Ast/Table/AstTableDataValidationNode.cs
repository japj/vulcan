using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Table
{
    [AstSchemaTypeBindingAttribute("TableDataValidationElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableDataValidationBaseNode : AstNode
    {
        #region Private Storage
        #endregion   // Private Storage

        #region Public Accessor Properties
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableDataValidationBaseNode()
        {
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
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
