using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationIsNullPatcherElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstIsNullPatcherNode : AstTransformationNode
    {
        #region Private Storage
        private VulcanCollection<AstIsNullPatcherColumnNode> _columns;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Column", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstIsNullPatcherColumnNode> Columns
        {
            get { return _columns; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstIsNullPatcherNode()
        {
            this._columns = new VulcanCollection<AstIsNullPatcherColumnNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Columns.Cast<AstNode>());
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationIsNullPatcherColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstIsNullPatcherColumnNode : AstTransformationNode
    {
        #region Private Storage
        private string _defaultValue;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstIsNullPatcherColumnNode() { }
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
