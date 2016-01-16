using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("DestinationElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDestinationNode : AstTransformationNode
    {
        #region Private Storage
        private Connection.AstConnectionNode _connection;
        private string _tableName;
        private DestinationAccessModeFacet _accessMode;
        private bool _tableLock;
        private bool _checkConstraints;
        private bool _keepIdentity;
        private bool _keepNulls;
        private int _rowsPerBatch;
        private int _maximumInsertCommitSize;
        private VulcanCollection<AstDataFlowColumnMappingNode> _maps;

        private bool _useStaging = false;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("TableName", "http://tempuri.org/vulcan2.xsd")]
        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public DestinationAccessModeFacet AccessMode
        {
            get { return _accessMode; }
            set { _accessMode = value; }
        }

        public bool TableLock
        {
            get { return _tableLock; }
            set { _tableLock = value; }
        }

        public bool CheckConstraints
        {
            get { return _checkConstraints; }
            set { _checkConstraints = value; }
        }

        public bool KeepIdentity
        {
            get { return _keepIdentity; }
            set { _keepIdentity = value; }
        }

        public bool KeepNulls
        {
            get { return _keepNulls; }
            set { _keepNulls = value; }
        }

        public int RowsPerBatch
        {
            get { return _rowsPerBatch; }
            set { _rowsPerBatch = value; }
        }

        public int MaximumInsertCommitSize
        {
            get { return _maximumInsertCommitSize; }
            set { _maximumInsertCommitSize = value; }
        }

        public bool UseStaging
        {
            get { return _useStaging; }
            set { _useStaging = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Map", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstDataFlowColumnMappingNode> Maps
        {
            get { return _maps; }
            set { _maps = value; }
        }

        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDestinationNode()
        {
            _maps = new VulcanCollection<AstDataFlowColumnMappingNode>();
            _useStaging = false;

        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Maps.Cast<AstNode>());
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

    public enum DestinationAccessModeFacet
    {
        Table,
        TableFastLoad,
    }
}
