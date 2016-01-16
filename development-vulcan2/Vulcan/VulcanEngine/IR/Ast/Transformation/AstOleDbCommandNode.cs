using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationOLEDBCommandElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstOleDbCommandNode : AstTransformationNode
    {
        #region Private Storage
        private Connection.AstConnectionNode _connection;
        private string _command;
        private VulcanCollection<AstDataFlowColumnMappingNode> _maps;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@ConnectionName")]
        public Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Map", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstDataFlowColumnMappingNode> Maps
        {
            get { return _maps; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstOleDbCommandNode()
        {
            this._maps = new VulcanCollection<AstDataFlowColumnMappingNode>();
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
}
