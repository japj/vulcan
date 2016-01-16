using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationLookupElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstLookupNode : AstTransformationNode
    {
        #region Private Storage
        private string _query;
        private VulcanCollection<AstLookupInputOutputNode> _inputs;    // TODO: This assumes that Inputs and Outputs can't cross-over
        private VulcanCollection<AstLookupInputOutputNode> _outputs;  // TODO: This assumes that Inputs and Outputs can't cross-over
        private Connection.AstConnectionNode _connection;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Input", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstLookupInputOutputNode> Inputs
        {
            get { return _inputs; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Output", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstLookupInputOutputNode> Outputs
        {
            get { return _outputs; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstLookupNode()
        {
            this._inputs = new VulcanCollection<AstLookupInputOutputNode>();
            this._outputs = new VulcanCollection<AstLookupInputOutputNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Inputs.Cast<AstNode>());
                children.AddRange(this.Outputs.Cast<AstNode>());
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationLookupInputOutputElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstLookupInputOutputNode : AstTransformationNode
    {
        #region Private Storage
        private string _remoteColumnName;
        private string _localColumnName;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public string LocalColumnName
        {
            get { return _localColumnName; }
            set { _localColumnName = value; }
        }

        public string RemoteColumnName
        {
            get { return _remoteColumnName; }
            set { _remoteColumnName = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstLookupInputOutputNode() { }
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
}
