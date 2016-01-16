using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Task
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ExecuteSqlTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstExecuteSQLTaskNode : AstTaskNode
    {
        #region Private Storage
        private bool _ExecuteDuringDesignTime;
        private Connection.AstConnectionNode _Connection;
        private ExecuteSQLTaskType _Type;
        private ExecuteSQLResultSet _ResultSet;
        private System.Data.IsolationLevel _isolationLevel;
        private string _Body;
        private VulcanCollection<AstParameterMappingTypeNode> _Results;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public bool ExecuteDuringDesignTime
        {
            get { return _ExecuteDuringDesignTime; }
            set { _ExecuteDuringDesignTime = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public Connection.AstConnectionNode Connection
        {
            get { return _Connection; }
            set { _Connection = value; }
        }

        public System.Data.IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
            set { _isolationLevel = (System.Data.IsolationLevel)Enum.Parse(typeof(System.Data.IsolationLevel), value.ToString()); }
        }

        public ExecuteSQLTaskType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        public ExecuteSQLResultSet ResultSet
        {
            get { return _ResultSet; }
            set { _ResultSet = value; }
        }

        public string Body
        {
            get { return _Body; }
            set { _Body = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Result", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstParameterMappingTypeNode> Results
        {
            get { return _Results; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstExecuteSQLTaskNode()
            : base()
        {
            _Results = new VulcanCollection<AstParameterMappingTypeNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Results.Cast<AstNode>());
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ParameterMappingElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstParameterMappingTypeNode : AstNode
    {
        #region Private Storage
        private string _ParameterName;
        private AstVariableNode _Variable;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public string ParameterName
        {
            get { return _ParameterName; }
            set { _ParameterName = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("VariableName", "http://tempuri.org/vulcan2.xsd")]
        public AstVariableNode Variable
        {
            get { return _Variable; }
            set { _Variable = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstParameterMappingTypeNode() : base() { }
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

    public enum ExecuteSQLTaskType
    {
        Expression,
        File,
    }

    public enum ExecuteSQLResultSet
    {
        None,
        SingleRow,
        Full,
        XML,
    }
}
