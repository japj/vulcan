using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Table
{
    public class AstTableKeyBaseNode : AstNamedNode
    {
        #region Private Storage
        private bool _padIndex;
        private bool _sortInTempdb;
        private bool _dropExisting;
        private bool _ignoreDupKey;
        private bool _online;
        private bool _clustered;
        private bool _unique;
        private VulcanCollection<AstTableKeyColumnNode> _columns;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public bool PadIndex
        {
            get { return _padIndex; }
            set { _padIndex = value; }
        }

        public bool SortInTempdb
        {
            get { return _sortInTempdb; }
            set { _sortInTempdb = value; }
        }

        public bool DropExisting
        {
            get { return _dropExisting; }
            set { _dropExisting = value; }
        }

        public bool IgnoreDupKey
        {
            get { return _ignoreDupKey; }
            set { _ignoreDupKey = value; }
        }

        public bool Online
        {
            get { return _online; }
            set { _online = value; }
        }

        public bool Clustered
        {
            get { return _clustered; }
            set { _clustered = value; }
        }

        public bool Unique
        {
            get { return _unique; }
            set { _unique = value; }
        }

        [AstXNameBindingAttribute("Column", "http://tempuri.org/vulcan2.xsd")]
        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableKeyColumnNode> Columns
        {
            get { return _columns; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableKeyBaseNode()
        {
            this._columns = new VulcanCollection<AstTableKeyColumnNode>();
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
        #endregion  // Validation    }

        public int ComparisonBytes
        {
            get
            {
                int comparisonBytes = 0;
                foreach (AstTableKeyColumnNode keyColumn in this.Columns)
                {
                    comparisonBytes += keyColumn.Column.Length;
                    // TODO: Add in size mapping for primitive types
                }
                return comparisonBytes;
            }
        }
    }

    [AstSchemaTypeBindingAttribute("TablePrimaryKeyElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTablePrimaryKeyNode : AstTableKeyBaseNode { }

    [AstSchemaTypeBindingAttribute("TableIdentityElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableIdentityNode : AstTableKeyBaseNode
    {
        #region Private Storage
        private int _seed;
        private int _increment;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public int Seed
        {
            get { return _seed; }
            set { _seed = value; }
        }

        public int Increment
        {
            get { return _increment; }
            set { _increment = value; }
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

    [AstSchemaTypeBindingAttribute("TableUniqueKeyElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableUniqueKeyNode : AstTableKeyBaseNode { }

    [AstSchemaTypeBindingAttribute("TableForeignKeyElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableForeignKeyNode : AstNamedNode
    {
        #region Private Storage
        private Dimension.AstDimensionNode _dimension;
        private VulcanCollection<AstTableForeignKeyColumnNode> _columns;
        #endregion // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("DimensionName", "http://tempuri.org/vulcan2.xsd")]
        [BrowsableAttribute(false)]
        public Dimension.AstDimensionNode Dimension
        {
            get { return _dimension; }
            set { _dimension = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Column", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstTableForeignKeyColumnNode> Columns
        {
            get { return _columns; }
        }

        #endregion // Public Accessor Properties

        #region Default Constructor
        public AstTableForeignKeyNode()
        {
            _columns = new VulcanCollection<AstTableForeignKeyColumnNode>();
        }
        #endregion // Default Constructor
    }

    [AstSchemaTypeBindingAttribute("TableForeignKeyColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableForeignKeyColumnNode : AstNode
    {
        #region Private Storage
        private string _columnName;
        private string _outputName;
        #endregion // Private Storage

        #region Public Accessor Properties
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public string OutputName
        {
            get { return _outputName; }
            set { _outputName = value; }
        }
        #endregion // Public Accessor Properties

        #region Default Constructor
        public AstTableForeignKeyColumnNode()
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

    [AstSchemaTypeBindingAttribute("TableKeyColumnReferenceElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableKeyColumnNode : AstNode
    {
        #region Private Storage
        private AstTableColumnBaseNode _column;
        private KeyColumnSortOrder _sortOrder;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("ColumnName", "http://tempuri.org/vulcan2.xsd")]
        public AstTableColumnBaseNode Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public KeyColumnSortOrder SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableKeyColumnNode() { }
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

    public enum KeyColumnSortOrder
    {
        ASC,
        DESC,
    }
}
