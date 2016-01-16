using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;
namespace VulcanEngine.IR.Ast.Task
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ContainerTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstContainerTaskNode : AstTaskNode
    {
        #region Private Storage
        private bool _log;
        private Connection.AstConnectionNode _logConnection;
        private ContainerConstraintMode _constraintMode = ContainerConstraintMode.Linear;
        private ContainerTransactionMode _transactionMode = ContainerTransactionMode.Join;
        private VulcanCollection<AstTaskNode> _tasks;
        private VulcanCollection<AstVariableNode> _variables;
        private VulcanCollection<Table.AstTableNode> _helperTables;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public bool Log
        {
            get { return _log; }
            set { _log = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("LogConnectionName", "http://tempuri.org/vulcan2.xsd")]
        public Connection.AstConnectionNode LogConnection
        {
            get { return _logConnection; }
            set { _logConnection = value; }
        }

        public ContainerTransactionMode TransactionMode
        {
            get { return _transactionMode; }
            set { _transactionMode = value; }
        }

        public ContainerConstraintMode ConstraintMode
        {
            get { return _constraintMode; }
            set { _constraintMode = value; }
        }

        
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Staging", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Container", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("ExecuteSQL", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("ETL", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("ExecutePackage", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("StoredProc", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Merge", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstTaskNode> Tasks
        {
            get { return _tasks; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Variable", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstVariableNode> Variables
        {
            get { return _variables; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("HelperTable", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Table.AstTableNode> HelperTables
        {
            get { return _helperTables; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstContainerTaskNode() : base() 
        {
            _tasks = new VulcanCollection<AstTaskNode>();
            _variables = new VulcanCollection<AstVariableNode>();
            _helperTables = new VulcanCollection<VulcanEngine.IR.Ast.Table.AstTableNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Tasks.Cast<AstNode>());
                children.AddRange(this.Variables.Cast<AstNode>());
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

    public enum ContainerTransactionMode
    {
        StartOrJoin,
        Join,
        NoTransaction
    };

    public enum ContainerConstraintMode
    {
        Linear,
        Parallel
    };
}
