using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationConditionalSplitElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstConditionalSplitNode : AstTransformationNode
    {
        #region Private Storage
        private VulcanCollection<AstConditionalSplitOutputNode> _outputs;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("ConditionalOutputPath", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstConditionalSplitOutputNode> Outputs
        {
            get { return _outputs; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstConditionalSplitNode()
        {
            this._outputs = new VulcanCollection<AstConditionalSplitOutputNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Outputs.Cast<AstNode>());
                children.Add(this.OutputPath);
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationConditionalSplitOutputElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstConditionalSplitOutputNode : AstETLOutputPathNode
    {
        #region Private Storage
        private string _Expression;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public string Expression
        {
          get { return _Expression; }
          set { _Expression = value; }
        }

        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstConditionalSplitOutputNode() { }
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationConditionalSplitOutputPathElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstConditionalSplitOutputPathNode : AstTransformationNode
    {
        #region Private Storage
        private VulcanCollection<AstTransformationNode> _transformations;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Destination", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstTransformationNode> Transformations
        {
            get { return _transformations; }
            set { _transformations = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstConditionalSplitOutputPathNode()
        {
            this._transformations = new VulcanCollection<AstTransformationNode>();
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
