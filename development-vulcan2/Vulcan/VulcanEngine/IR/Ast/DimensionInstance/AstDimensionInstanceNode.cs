using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.IR.Ast.Dimension;

using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Ssas;

namespace VulcanEngine.IR.Ast.DimensionInstance
{
    [AstScopeBoundaryAttribute()]
    public class AstDimensionInstanceNode : AstNamedNode
    {
        #region Private Storage
        private AstDimensionNode _dimension;
        private string _description;
        private SsasAggregationUsage _allMemberAggregationUsage;
        private SsasHierarchyUniqueNameStyle _hierarchyUniqueNameStyle;
        private SsasMemberUniqueNameStyle _memberUniqueNameStyle;
        private bool _visible;
        private VulcanCollection<AstDimensionInstanceAttributeNode> _attributeProperties;
        private VulcanCollection<AstDimensionInstanceHierarchyNode> _hierarchyProperties;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("DimensionName", "http://tempuri.org/vulcan2.xsd")]
        public AstDimensionNode Dimension
        {
            get { return _dimension; }
            set { _dimension = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public SsasAggregationUsage AllMemberAggregationUsage
        {
            get { return _allMemberAggregationUsage; }
            set { _allMemberAggregationUsage = value; }
        }

        public SsasHierarchyUniqueNameStyle HierarchyUniqueNameStyle
        {
            get { return _hierarchyUniqueNameStyle; }
            set { _hierarchyUniqueNameStyle = value; }
        }

        public SsasMemberUniqueNameStyle MemberUniqueNameStyle
        {
            get { return _memberUniqueNameStyle; }
            set { _memberUniqueNameStyle = value; }
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public VulcanCollection<AstDimensionInstanceAttributeNode> AttributeProperties
        {
            get { return _attributeProperties; }
        }

        public VulcanCollection<AstDimensionInstanceHierarchyNode> HierarchyProperties
        {
            get { return _hierarchyProperties; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDimensionInstanceNode()
        {
            this._attributeProperties = new VulcanCollection<AstDimensionInstanceAttributeNode>();
            this._hierarchyProperties = new VulcanCollection<AstDimensionInstanceHierarchyNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.AttributeProperties.Cast<AstNode>());
                children.AddRange(this.HierarchyProperties.Cast<AstNode>());
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
