using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationAutoNullPatcherElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstAutoNullPatcherNode : AstTransformationNode
    {
        #region Private Storage
        private Fact.AstFactNode _factTable;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("FactTableName", "http://tempuri.org/vulcan2.xsd")]
        public Fact.AstFactNode FactTable
        {
            get { return _factTable; }
            set { _factTable = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstAutoNullPatcherNode() { }
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
