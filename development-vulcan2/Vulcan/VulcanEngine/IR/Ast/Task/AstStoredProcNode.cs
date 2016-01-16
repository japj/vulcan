using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Task
{
    [AstSchemaTypeBindingAttribute("StoredProcElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstStoredProcNode : AstTaskNode
    {
        #region Private Storage
        private bool _executeDuringDesignTime;
        private Connection.AstConnectionNode _connection;
        private VulcanCollection<AstStoredProcColumnNode> _columns;
        private string _body;
        #endregion   // Private Storage

        #region Public Accessor Properties

        public bool ExecuteDuringDesignTime
        {
            get { return _executeDuringDesignTime; }
            set { _executeDuringDesignTime = value; }
        }

        [AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public VulcanCollection<AstStoredProcColumnNode> Columns
        {
            get { return _columns; }
        }

        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstStoredProcNode()
        {
            this._columns = new VulcanCollection<AstStoredProcColumnNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Columns.Cast<AstNode>());
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

    [AstSchemaTypeBindingAttribute("StoredProcColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstStoredProcColumnNode : Table.AstTableColumnNode
    {
        #region Private Storage
        private bool _isOutput;
        #endregion   // Private Storage

        #region Public Accessor Properties
        
        public bool IsOutput
        {
            get { return _isOutput; }
            set { _isOutput = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstStoredProcColumnNode() : base() { }
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
