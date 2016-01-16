using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationTermLookupElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTermLookupNode : AstTransformationNode
    {
        #region Private Storage
        private Connection.AstConnectionNode _connection;
        private bool _isCaseSensitive;
        private string _refTermColumnName;
        private string _refTermTableName;
        private VulcanCollection<AstTermLookupColumnNode> _inputColumns;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public bool IsCaseSensitive
        {
            get { return _isCaseSensitive; }
            set { _isCaseSensitive = value; }
        }

        public string RefTermColumnName
        {
            get { return _refTermColumnName; }
            set { _refTermColumnName = value; }
        }

        public string RefTermTableName
        {
            get { return _refTermTableName; }
            set { _refTermTableName = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("InputColumn", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstTermLookupColumnNode> InputColumns
        {
            get { return _inputColumns; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTermLookupNode()
        {
            this._inputColumns = new VulcanCollection<AstTermLookupColumnNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.InputColumns.Cast<AstNode>());
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationTermLookColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTermLookupColumnNode : AstTransformationNode
    {
        #region Private Storage
        private string _inputColumnName;  // TODO: Should this be a reference
        private string _inputColumnUsageType;  // TODO: Should this be a reference
        #endregion   // Private Storage

        #region Public Accessor Properties
        // TODO: This refers to a lineage ID rather than a column - should we rename it?
        public string InputColumnName
        {
            get { return _inputColumnName; }
            set { _inputColumnName = value; }
        }

        public string InputColumnUsageType
        {
            get { return _inputColumnUsageType; }
            set { _inputColumnUsageType = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTermLookupColumnNode() { }
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

    public enum TermLookupColumnUsageType
    {
        PassThrough,
        Lookup,
        BothPassThroughAndLookup,
    }
}
