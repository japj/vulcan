using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Fact
{
    [AstSchemaTypeBindingAttribute("FactElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstFactNode : Table.AstTableNode
    {
        #region Private Storage
        #endregion   // Private Storage

        #region Public Accessor Properties
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstFactNode() : base()   { }
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
