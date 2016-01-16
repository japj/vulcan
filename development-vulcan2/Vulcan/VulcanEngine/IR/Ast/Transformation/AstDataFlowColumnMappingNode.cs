using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("GlobalColumnMapElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDataFlowColumnMappingNode : AstNode
    {
        #region Private Storage
        private string _sourceName;
        private string _destinationName;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public string SourceName
        {
            get { return _sourceName; }
            set { _sourceName = value; }
        }

        public string DestinationName
        {
            get { return _destinationName; }
            set { _destinationName = value; }
        }
        #endregion   // Public Accessor Properties

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
