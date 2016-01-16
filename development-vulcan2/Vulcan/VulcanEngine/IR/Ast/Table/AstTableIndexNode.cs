using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Table
{
    [AstSchemaTypeBindingAttribute("TableIndexElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableIndexNode : AstNamedNode
    {
        #region Private Storage
        private bool _padIndex;
        private bool _sortInTempdb;
        private bool _dropExisting;
        private bool _ignoreDupKey;
        private bool _online;
        private bool _clustered;
        private bool _unique;
        private VulcanCollection<AstTableIndexColumnNode> _columns;
        private VulcanCollection<AstTableColumnBaseNode> _leafs;
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
        public VulcanCollection<AstTableIndexColumnNode> Columns
        {
            get { return _columns; }
        }

        [AstXNameBindingAttribute("Leaf", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstTableColumnBaseNode> Leafs
        {
            get { return _leafs; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableIndexNode()
        {
            this._columns = new VulcanCollection<AstTableIndexColumnNode>();
            this._leafs = new VulcanCollection<AstTableColumnBaseNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Columns.Cast<AstNode>());
                children.AddRange(this.Leafs.Cast<AstNode>());
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

    [AstSchemaTypeBindingAttribute("TableIndexColumnReferenceElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableIndexColumnNode : AstNode
    {
        #region Private Storage
        private AstTableColumnBaseNode _column;
        private IndexColumnSortOrder _sortOrder;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("ColumnName", "http://tempuri.org/vulcan2.xsd")]
        public AstTableColumnBaseNode Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public IndexColumnSortOrder SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableIndexColumnNode() { }
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


    public enum IndexColumnSortOrder
    {
        ASC,
        DESC,
    }
}
