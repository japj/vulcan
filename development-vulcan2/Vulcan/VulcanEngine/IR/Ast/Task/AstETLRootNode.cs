using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Task
{
    [AstScopeBoundaryAttribute()]
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ETLRootElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstETLRootNode : Task.AstTaskNode
    {
        #region Private Storage
        private bool _delayValidation;
        private VulcanCollection<Transformation.AstTransformationNode> _transformations;
        private System.Data.IsolationLevel _isolationLevel;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public bool DelayValidation
        {
            get { return _delayValidation; }
            set { _delayValidation = value; }
        }

        public System.Data.IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
            set { _isolationLevel = (System.Data.IsolationLevel)Enum.Parse(typeof(System.Data.IsolationLevel), value.ToString()); }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("QuerySource", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("XmlSource", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Destination", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Transformation.AstTransformationNode> Transformations
        {
            get { return _transformations; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstETLRootNode()
        {
            this._transformations = new VulcanCollection<VulcanEngine.IR.Ast.Transformation.AstTransformationNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Transformations.Cast<AstNode>());
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
