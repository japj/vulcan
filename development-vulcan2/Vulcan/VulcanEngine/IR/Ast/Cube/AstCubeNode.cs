using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Cube
{
    public class AstCubeNode : AstNamedNode
    {
        // Attributes
        private bool _visible;
        private string _description;
        private string _aggregationPrefix;
        private string _defaultMeasure;
        private string _errorConfiguration;
        private string _estimatedRows;
        private string _language;
        private string _processingMode;
        private string _processingPriority;
        private string _collation;
        private string _scriptCacheProcessingMode;
        private string _scriptErrorHandlingMode;
        private string _proactiveCaching;
        private string _storageMode;
        private string _storageLocation;
        // Elements
        private VulcanCollection<DimensionInstance.AstDimensionInstanceNode> _dimensionInstances;
        private VulcanCollection<MeasureGroup.AstMeasureGroupNode> _measureGroups;
        private VulcanCollection<AstDimensionUsageNode> _dimensionUsages;
        private VulcanCollection<AstKpiNode> _kpis;
        private VulcanCollection<IAstActionNode> _actions;
        //private VulcanCollection<ASTPartitionNode> _partitions;
        //private VulcanCollection<ASTPerspectiveNode> _perspectives;

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
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

        public string DefaultMeasure
        {
            get { return _defaultMeasure; }
            set { _defaultMeasure = value; }
        }

        public string ErrorConfiguration
        {
            get { return _errorConfiguration; }
            set { _errorConfiguration = value; }
        }

        public string EstimatedRows
        {
            get { return _estimatedRows; }
            set { _estimatedRows = value; }
        }

        public string Language
        {
            get { return _language; }
            set { _language = value; }
        }

        public string ProcessingMode
        {
            get { return _processingMode; }
            set { _processingMode = value; }
        }

        public string ProcessingPriority
        {
            get { return _processingPriority; }
            set { _processingPriority = value; }
        }

        public string Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        public string ScriptCacheProcessingMode
        {
            get { return _scriptCacheProcessingMode; }
            set { _scriptCacheProcessingMode = value; }
        }

        public string ScriptErrorHandlingMode
        {
            get { return _scriptErrorHandlingMode; }
            set { _scriptErrorHandlingMode = value; }
        }

        public string ProactiveCaching
        {
            get { return _proactiveCaching; }
            set { _proactiveCaching = value; }
        }

        public string StorageMode
        {
            get { return _storageMode; }
            set { _storageMode = value; }
        }

        public string StorageLocation
        {
            get { return _storageLocation; }
            set { _storageLocation = value; }
        }

        public VulcanCollection<DimensionInstance.AstDimensionInstanceNode> DimensionInstances
        {
            get { return _dimensionInstances; }
            set { _dimensionInstances = value; }
        }

        public VulcanCollection<MeasureGroup.AstMeasureGroupNode> MeasureGroups
        {
            get { return _measureGroups; }
            set { _measureGroups = value; }
        }

        public VulcanCollection<AstDimensionUsageNode> DimensionUsages
        {
            get { return _dimensionUsages; }
            set { _dimensionUsages = value; }
        }

        public VulcanCollection<AstKpiNode> Kpis
        {
            get { return _kpis; }
            set { _kpis = value; }
        }

        public VulcanCollection<IAstActionNode> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }

        public override IList<VulcanEngine.Common.ValidationItem> Validate()
        {
            throw new NotImplementedException();
        }
    }
}
