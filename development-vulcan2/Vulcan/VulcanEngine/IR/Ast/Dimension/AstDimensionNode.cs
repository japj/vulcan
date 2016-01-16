using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Table;

namespace VulcanEngine.IR.Ast.Dimension
{
    [AstSchemaTypeBindingAttribute("DimensionElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDimensionNode : Table.AstTableNode
    {
        #region Private Storage
        private VulcanCollection<AstAttributeNode> _attributes;
        private VulcanCollection<AstAttributeRelationshipNode> _relationships;
        private VulcanCollection<AstDimensionHierarchyNode> _attributeHierachies;
        #endregion  // Private Storage

        #region Public Accessor Properties

        [BrowsableAttribute(false)]
        public VulcanCollection<AstAttributeNode> Attributes
        {
            get { return _attributes; }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstAttributeRelationshipNode> Relationships
        {
            get { return _relationships; }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstDimensionHierarchyNode> AttributeHierachies
        {
            get { return _attributeHierachies; }
        }
        #endregion  // Public Accessor Properties

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.AttributeHierachies.Cast<AstNode>());
                children.AddRange(this.Attributes.Cast<AstNode>());
                children.AddRange(this.Relationships.Cast<AstNode>());
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

        #region Constructors
        public AstDimensionNode() : base()
        {
            this._attributes = new VulcanCollection<AstAttributeNode>();
            this._relationships = new VulcanCollection<AstAttributeRelationshipNode>();
            this._attributeHierachies = new VulcanCollection<AstDimensionHierarchyNode>();
        }
        #endregion // Constructors
    }
}
