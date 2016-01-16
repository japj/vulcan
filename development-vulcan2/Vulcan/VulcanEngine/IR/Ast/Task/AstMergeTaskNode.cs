using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Connection;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Task
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("MergeTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstMergeTaskNode : AstTaskNode
    {
        #region Private Storage
        private AstConnectionNode _connection;
        private string _sourceName;
        private AstTableKeyBaseNode _targetKey;
        private bool _bUpdateTargetTable;
        private VulcanCollection<AstMergeColumnNode> _columns;
        #endregion  // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        [AstXNameBindingAttribute("SourceTableName", "http://tempuri.org/vulcan2.xsd")]
        public string SourceName
        {
            get { return _sourceName; }
            set { _sourceName = value; }
        }

        [AstXNameBindingAttribute("TargetConstraintName", "http://tempuri.org/vulcan2.xsd")]
        public AstTableKeyBaseNode TargetConstraint
        {
            get { return _targetKey; }
            set { _targetKey = value; }
        }

        [AstXNameBindingAttribute("UnspecifiedColumnDefaultUsageType", "http://tempuri.org/vulcan2.xsd")]
        public string UnspecifiedColumnDefaultUsageType { get; set; }

        public bool UpdateTargetTable
        {
            get { return _bUpdateTargetTable; }
            set { _bUpdateTargetTable = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Column", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstMergeColumnNode> Columns
        {
            get { return _columns; }
        }

        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstMergeTaskNode() : base() 
        {
            _columns = new VulcanCollection<AstMergeColumnNode>();
        }
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("MergeColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstMergeColumnNode : AstNode
    {
        #region Private Storage
        private string _columnName;
        private string _columnUsage;
        #endregion // Private Storage

        #region Public Accessor Properties
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public string ColumnUsage
        {
            get { return _columnUsage; }
            set { _columnUsage = value; }
        }
        #endregion // Public Accessor Properties

        #region Default Constructor
        public AstMergeColumnNode()
        {
        }
        #endregion // Default Constructor

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
