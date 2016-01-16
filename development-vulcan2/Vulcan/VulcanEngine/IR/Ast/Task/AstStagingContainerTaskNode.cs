using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Task
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("StagingTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstStagingContainerTaskNode : AstContainerTaskNode
    {
        #region Private Storage
        private Ast.Table.AstTableNode _table;
        private string _createAs;
        private Connection.AstConnectionNode _stagingConnection;
        private VulcanCollection<string> _dropConstraints;
        private bool _bUseStaticSource;
        #endregion  // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("StagingConnection", "http://tempuri.org/vulcan2.xsd")]
        public Connection.AstConnectionNode StagingConnection
        {
            get { return _stagingConnection; }
            set { _stagingConnection = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Table", "http://tempuri.org/vulcan2.xsd")]
        public Ast.Table.AstTableNode Table
        {
            get { return _table; }
            set { _table = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("DropConstraint", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<string> DropConstraints
        {
            get { return _dropConstraints; }
        }

        public bool UseStaticSource
        {
            get { return _bUseStaticSource; }
            set { _bUseStaticSource = value; }
        }

        public string CreateAs
        {
            get { return _createAs; }
            set { _createAs = value; }
        }
        #endregion  // Public Accessor Properties

        #region Default Constructor
        public AstStagingContainerTaskNode() : base() 
        {
            _dropConstraints = new VulcanCollection<string>();
        }
        #endregion  // Default Constructor

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
