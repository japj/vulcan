using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Ssas;

namespace VulcanEngine.IR.Ast.Dimension
{
    [AstSchemaTypeBindingAttribute("AttributeElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstAttributeNode : AstDimensionNamedBaseNode
    {
        // TODO: There are new properties to add in SSAS 2008

        #region Private Storage
        private SsasAttributeUsage _usage;
        private SsasOrderBy _orderBy;
        private AstAttributeNode _orderByAttribute;
        private bool _memberNameUnique;
        private int _estimatedCount;
        private bool _isAggregatable;
        private bool _attributeHierarchyEnabled;
        private bool _attributeHierarchyOrdered;
        private SsasOptimizedState _attributeHierarchyOptimizedState;
        private bool _attributeHierarchyVisible;
        private string _attributeHierarchyDisplayFolder;
        // TODO: Should these be column definitions or references?
        // Definitions hits the common case nicely, but it prevents the reuse of columns across multiple attributes - maybe we support both models?
        private VulcanCollection<AstAttributeColumnNode> _columns;
        private VulcanCollection<AstAttributeKeyColumnNode> _keyColumns;
        private AstAttributeNameColumnNode _nameColumn;
        private AstAttributeValueColumnNode _valueColumn;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public SsasAttributeUsage Usage
        {
            get { return _usage; }
            set { _usage = value; }
        }

        public SsasOrderBy OrderBy
        {
            get { return _orderBy; }
            set { _orderBy = value; }
        }

        [AstXNameBindingAttribute("OrderByAttributeName", "http://tempuri.org/vulcan2.xsd")]
        public AstAttributeNode OrderByAttribute
        {
            get { return _orderByAttribute; }
            set { _orderByAttribute = value; }
        }

        public bool MemberNameUnique
        {
            get { return _memberNameUnique; }
            set { _memberNameUnique = value; }
        }

        public int EstimatedCount
        {
            get { return _estimatedCount; }
            set { _estimatedCount = value; }
        }

        public bool IsAggregatable
        {
            get { return _isAggregatable; }
            set { _isAggregatable = value; }
        }

        public bool AttributeHierarchyEnabled
        {
            get { return _attributeHierarchyEnabled; }
            set { _attributeHierarchyEnabled = value; }
        }

        public bool AttributeHierarchyOrdered
        {
            get { return _attributeHierarchyOrdered; }
            set { _attributeHierarchyOrdered = value; }
        }

        public SsasOptimizedState AttributeHierarchyOptimizedState
        {
            get { return _attributeHierarchyOptimizedState; }
            set { _attributeHierarchyOptimizedState = value; }
        }

        public bool AttributeHierarchyVisible
        {
            get { return _attributeHierarchyVisible; }
            set { _attributeHierarchyVisible = value; }
        }

        public string AttributeHierarchyDisplayFolder
        {
            get { return _attributeHierarchyDisplayFolder; }
            set { _attributeHierarchyDisplayFolder = value; }
        }

        [AstXNameBindingAttribute("KeyColumn", "http://tempuri.org/vulcan2.xsd")]
        [BrowsableAttribute(false)]
        public VulcanCollection<AstAttributeKeyColumnNode> KeyColumns
        {
            get { return _keyColumns; }
        }

        [BrowsableAttribute(false)]
        public AstAttributeNameColumnNode NameColumn
        {
            get { return _nameColumn; }
            set { _nameColumn = value; VulcanOnPropertyChanged("Columns"); }
        }

        [BrowsableAttribute(false)]
        public AstAttributeValueColumnNode ValueColumn
        {
            get { return _valueColumn; }
            set { _valueColumn = value; VulcanOnPropertyChanged("Columns"); }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstAttributeColumnNode> Columns
        {
            get
            {
                _columns.Clear();

                if (_nameColumn != null)
                {
                    _columns.Add(NameColumn);
                }

                if (_valueColumn != null)
                {
                    _columns.Add(ValueColumn);
                }

                foreach (AstAttributeKeyColumnNode keyColumn in _keyColumns)
                {
                    _columns.Add(keyColumn);
                }

                return _columns;
            }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstAttributeNode() : base()
        {
            this._keyColumns = new VulcanCollection<AstAttributeKeyColumnNode>();
            this._columns = new VulcanCollection<AstAttributeColumnNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.KeyColumns.Cast<AstNode>());
                children.Add(this.NameColumn);
                children.Add(this.ValueColumn);
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

        void _keyColumns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            VulcanOnPropertyChanged("Columns");
        }
    }
}
