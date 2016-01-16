using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationSortElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstSortNode : AstTransformationNode
    {
        #region Private Storage
        private bool _eliminateDuplicates;
        private int _maximumThreads;
        private VulcanCollection<AstSortInputColumnNode> _inputColumns;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public bool EliminateDuplicates
        {
            get { return _eliminateDuplicates; }
            set { _eliminateDuplicates = value; }
        }

        public int MaximumThreads
        {
            get { return _maximumThreads; }
            set { _maximumThreads = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("InputColumn", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstSortInputColumnNode> InputColumns
        {
            get { return _inputColumns; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstSortNode()
        {
            this._inputColumns = new VulcanCollection<AstSortInputColumnNode>();
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationSortInputColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstSortInputColumnNode : AstTransformationNode
    {
        #region Private Storage
        private SortColumnUsageType _inputColumnUsageType;
        private string _inputColumnName; // TODO: should this be a reference?
        private SortType _sortType;
        private VulcanCollection<SortComparisonFlag> _comparisonFlags;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public SortColumnUsageType InputColumnUsageType
        {
            get { return _inputColumnUsageType; }
            set { _inputColumnUsageType = value; }
        }

        // TODO: This refers to a lineage ID rather than a column - should we rename it?
        public string InputColumnName
        {
            get { return _inputColumnName; }
            set { _inputColumnName = value; }
        }

        public SortType SortType
        {
            get { return _sortType; }
            set { _sortType = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("ComparisonFlag", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<SortComparisonFlag> ComparisonFlags
        {
            get { return _comparisonFlags; }
            set { _comparisonFlags = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstSortInputColumnNode()
        {
            this._comparisonFlags = new VulcanCollection<SortComparisonFlag>();
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

    public enum SortComparisonFlag
    {
        IgnoreCase,
		IgnoreKanaType,
		IgnoreNonspacingCharacters,
		IgnoreCharacterWidth,
		IgnoreSymbols,
		SortPunctuationAsSymbols,
    }

    public enum SortType
    {
        ASC,
        DESC,
    }

    public enum SortColumnUsageType
    {
        PassThrough,
        SortColumn,
    }
}
