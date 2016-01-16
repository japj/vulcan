using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    public class AstSourceNode : Transformation.AstTransformationNode { }

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ETLQuerySourceElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstQuerySourceNode : AstSourceNode
    {
        #region Private Storage
        private Connection.AstConnectionNode _connection;
        private VulcanCollection<AstQuerySourceParameterNode> _parameters;
        private AstETLQueryNode _queryNode;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Connection", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public string Query
        {
            get { return _queryNode.Query; }
            set { _queryNode.Query = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Query", "http://tempuri.org/vulcan2.xsd")]
        public AstETLQueryNode QueryNode
        {
            get { return _queryNode; }
            set { _queryNode = value; }
        }

        public bool EvaluateAsExpression
        {
            get { return _queryNode.EvaluateAsExpression; }
            set { _queryNode.EvaluateAsExpression = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Parameter", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstQuerySourceParameterNode> Parameters
        {
            get { return _parameters; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstQuerySourceNode()
        {
            this._parameters = new VulcanCollection<AstQuerySourceParameterNode>();
            this._queryNode = new AstETLQueryNode();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Parameters.Cast<AstNode>());
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ETLQueryElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstETLQueryNode : AstRootNode
    {
        #region Private Storage
        private bool _evaluateAsExpression;
        private string _query;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("EvaluateAsExpression", "http://tempuri.org/vulcan2.xsd")]
        public bool EvaluateAsExpression
        {
            get { return _evaluateAsExpression; }
            set { _evaluateAsExpression = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "text()")]
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstETLQueryNode() { }
        #endregion   // Default Constructor

        #region Validation
        #endregion  // Validation
    }

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ETLQueryParameterElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstQuerySourceParameterNode : AstNamedNode
    {
        #region Private Storage
        private Task.AstVariableNode _variable;
        #endregion   // Private Storage

        #region Public Accessor Properties

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("VariableName", "http://tempuri.org/vulcan2.xsd")]
        public Task.AstVariableNode Variable
        {
            get { return _variable; }
            set { _variable = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstQuerySourceParameterNode() { }
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ETLXmlSourceElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstXmlSourceNode : AstSourceNode
    {
        #region Private Storage
        private string _xmlData;
        private string _xmlSchemaDefinition;
        private bool _useInlineSchema;
        private XmlSourceDataAccessMode _xmlDataAccessMode;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public string XMLData
        {
            get { return _xmlData; }
            set { _xmlData = value; }
        }

        public string XMLSchemaDefinition
        {
            get { return _xmlSchemaDefinition; }
            set { _xmlSchemaDefinition = value; }
        }

        public bool UseInlineSchema
        {
            get { return _useInlineSchema; }
            set { _useInlineSchema = value; }
        }

        public XmlSourceDataAccessMode XMLDataAccessMode
        {
            get { return _xmlDataAccessMode; }
            set { _xmlDataAccessMode = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstXmlSourceNode() { }
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

    public enum XmlSourceDataAccessMode
    {
        XMLFileLocation,
        XMLFileFromVariable,
        XMLDataFromVariable,
    }
}
