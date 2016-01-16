using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Ssas;

namespace VulcanEngine.IR.Ast.MeasureGroup
{
    [AstScopeBoundaryAttribute()]
    [AstSchemaTypeBindingAttribute("MeasureGroupElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstMeasureGroupNode : AstNamedNode
    {
        #region Private Storage
        private bool _visible;
        private Fact.AstFactNode _factTable;
        private string _description;
        private string _aggregationPrefix;
        private AstMeasureNode _defaultMeasure;
        private SsasErrorConfiguration _errorConfiguration;
        private int _estimatedRows;
        private int _estimatedSize;
        private bool _ignoreUnrelatedDimensions;
        private SsasProcessingMode _processingMode;
        private int _processingPriority;
        private SsasMeasureGroupType _type;
        private SsasDataAggregation _dataAggregation;
        private string _proactiveCaching;
        private SsasStorageMode _storageMode;
        private string _storageLocation;
        private VulcanCollection<AstMeasureNode> _measures;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("FactTableName", "http://tempuri.org/vulcan2.xsd")]
        public Fact.AstFactNode FactTable
        {
            get { return _factTable; }
            set { _factTable = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string AggregationPrefix
        {
            get { return _aggregationPrefix; }
            set { _aggregationPrefix = value; }
        }

        // TODO: This may not work with scoped naming unless you do MeasureGroup.Measure - a little unnatural
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("DefaultMeasureName", "http://tempuri.org/vulcan2.xsd")]
        public AstMeasureNode DefaultMeasure
        {
            get { return _defaultMeasure; }
            set { _defaultMeasure = value; }
        }

        // TODO: This should be a class for the cases where it's custom
        public SsasErrorConfiguration ErrorConfiguration
        {
            get { return _errorConfiguration; }
            set { _errorConfiguration = value; }
        }

        public int EstimatedRows
        {
            get { return _estimatedRows; }
            set { _estimatedRows = value; }
        }

        public int EstimatedSize
        {
            get { return _estimatedSize; }
            set { _estimatedSize = value; }
        }

        public bool IgnoreUnrelatedDimensions
        {
            get { return _ignoreUnrelatedDimensions; }
            set { _ignoreUnrelatedDimensions = value; }
        }

        public SsasProcessingMode ProcessingMode
        {
            get { return _processingMode; }
            set { _processingMode = value; }
        }

        public int ProcessingPriority
        {
            get { return _processingPriority; }
            set { _processingPriority = value; }
        }

        public SsasMeasureGroupType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public SsasDataAggregation DataAggregation
        {
            get { return _dataAggregation; }
            set { _dataAggregation = value; }
        }

        // TODO: May need more complex representation for this
        public string ProactiveCaching
        {
            get { return _proactiveCaching; }
            set { _proactiveCaching = value; }
        }

        public SsasStorageMode StorageMode
        {
            get { return _storageMode; }
            set { _storageMode = value; }
        }

        // TODO: Can we do some sort of file path selection here?
        public string StorageLocation
        {
            get { return _storageLocation; }
            set { _storageLocation = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Measure", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstMeasureNode> Measures
        {
            get { return _measures; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstMeasureGroupNode()
        {
            this._measures = new VulcanCollection<AstMeasureNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Measures.Cast<AstNode>());
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
}
