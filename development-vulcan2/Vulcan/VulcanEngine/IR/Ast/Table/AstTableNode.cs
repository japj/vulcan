using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Table
{
    [AstScopeBoundaryAttribute()]
    [AstSchemaTypeBindingAttribute("TableElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableNode : AstNamedNode, IPackageRootNode
    {
        #region Private Storage
        private bool _emit;
        private bool _executeDuringDesignTime;
        // Where a table resides.
        private Connection.AstConnectionNode _connection;
        // Containers for pieces of data.
        private VulcanCollection<AstTableColumnBaseNode> _columns;
        // Uniquely identifying rows via a set of columns.
        private VulcanCollection<AstTableKeyBaseNode> _keys;
        private VulcanCollection<AstTableForeignKeyNode> _foreignKeys;
        // Input to the table.
        private VulcanCollection<AstTableSourceBaseNode> _sources;
        // How data is retrieved from the table.
        private VulcanCollection<AstTableLookupBaseNode> _lookups;
        // Sorted orders for columns to speed up table searching.
        private VulcanCollection<AstTableIndexNode> _indexes;
        private VulcanCollection<AstTableDataValidationBaseNode> _dataValidations;
        #endregion   // Private Storage

        #region Public Accessor Properties

        public bool Emit
        {
            get { return _emit; }
            set { _emit = value; }
        }

        [AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public bool ExecuteDuringDesignTime
        {
            get { return _executeDuringDesignTime; }
            set { _executeDuringDesignTime = value; }
        }

        [AstXNameBindingAttribute("Column", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("Dimension", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("TableReference", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("HashedKey", "http://tempuri.org/vulcan2.xsd")]
        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableColumnBaseNode> Columns
        {
            get { return _columns; }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableKeyBaseNode> Keys
        {
            get { return _keys; }
        }

        [AstXNameBindingAttribute("ForeignKey", "http://tempuri.org/vulcan2.xsd")]
        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableForeignKeyNode> ForeignKeys
        {
            get { return _foreignKeys; }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableSourceBaseNode> Sources
        {
            get { return _sources; }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableLookupBaseNode> Lookups
        {
            get { return _lookups; }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableIndexNode> Indexes
        {
            get { return _indexes; }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableDataValidationBaseNode> DataValidations
        {
            get { return _dataValidations; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableNode()
        {
            this._columns = new VulcanCollection<AstTableColumnBaseNode>();
            this._keys = new VulcanCollection<AstTableKeyBaseNode>();
            this._foreignKeys = new VulcanCollection<AstTableForeignKeyNode>();
            this._sources = new VulcanCollection<AstTableSourceBaseNode>();
            this._lookups = new VulcanCollection<AstTableLookupBaseNode>();
            this._indexes = new VulcanCollection<AstTableIndexNode>();
            this._dataValidations = new VulcanCollection<AstTableDataValidationBaseNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.Add(this.Connection);
                children.AddRange(this.Columns.Cast<AstNode>());
                children.AddRange(this.Keys.Cast<AstNode>());
                children.AddRange(this.Sources.Cast<AstNode>());
                children.AddRange(this.Lookups.Cast<AstNode>());
                children.AddRange(this.Indexes.Cast<AstNode>());
                children.AddRange(this.DataValidations.Cast<AstNode>());
                return children;
            }
        }

        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();

            foreach (AstNode child in this.Children)
            {
                validationItems.AddRange(child.Validate());
            }

            // TODO: Add Validation for Name

            return validationItems;
        }
        #endregion  // Validation

        public AstTableKeyBaseNode PreferredKey
        {
            get
            {
                AstTableKeyBaseNode preferredKey = null;
                foreach (AstTableKeyBaseNode candidate in this.Keys)
                {
                    if (candidate.Clustered)
                    {
                        return candidate;
                    }
                    if (preferredKey == null)
                    {
                        preferredKey = candidate;
                    }
                    if (candidate.ComparisonBytes < preferredKey.ComparisonBytes)
                    {
                        preferredKey = candidate;
                    }
                }
                return preferredKey;
            }
        }
    }
}
