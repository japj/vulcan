using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationUnionAllElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstUnionAllNode : AstTransformationNode
    {
        #region Private Storage
        private VulcanCollection<AstUnionAllInputPathNode> _inputPathCollection;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("UnionInputPath", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstUnionAllInputPathNode> InputPathCollection
        {
          get { return _inputPathCollection; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstUnionAllNode()
        {
            this._inputPathCollection = new VulcanCollection<AstUnionAllInputPathNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this._inputPathCollection.Cast<AstNode>());
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationUnionAllInputPathElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstUnionAllInputPathNode : AstETLInputPathNode
    {
        #region Private Storage
        private VulcanCollection<AstDataFlowColumnMappingNode> _maps;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Map", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstDataFlowColumnMappingNode> Maps
        {
          get { return _maps; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstUnionAllInputPathNode()
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
