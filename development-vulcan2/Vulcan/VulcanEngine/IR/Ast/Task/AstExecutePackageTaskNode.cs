using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Task
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ExecutePackageTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstExecutePackageTaskNode : AstTaskNode
    {
        #region Private Storage
        private string _relativePath;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public string RelativePath
        {
            get { return _relativePath; }
            set { _relativePath = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstExecutePackageTaskNode() : base() { }
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
