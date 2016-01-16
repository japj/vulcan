using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.IR.Ast.Dimension;

using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Ssas;

namespace VulcanEngine.IR.Ast.DimensionInstance
{
    // TODO: Is this right?  Should be be doing inclusive or exclusive lists on attribute references?
    public class AstDimensionInstanceAttributeNode : AstNamedNode
    {
        #region Private Storage
        private AstAttributeNode _attribute;
        private SsasAggregationUsage _aggregationUsage;
        private bool _attributeHierarchyEnabled;
        private SsasOptimizedState _attributeHierarchyOptimizedState;
        private bool _attributeHierarchyVisible;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("AttributeName", "http://tempuri.org/vulcan2.xsd")]
        public AstAttributeNode Attribute
        {
            get { return _attribute; }
            set { _attribute = value; }
        }

        public SsasAggregationUsage AggregationUsage
        {
            get { return _aggregationUsage; }
            set { _aggregationUsage = value; }
        }

        public bool AttributeHierarchyEnabled
        {
            get { return _attributeHierarchyEnabled; }
            set { _attributeHierarchyEnabled = value; }
        }

        public SsasOptimizedState AttributeHierarchyOptimizedState
        {
            get { return _attributeHierarchyOptimizedState; }
            set { _attributeHierarchyOptimizedState = value; }
        }

        public bool AttributeHierarchyVisible
        {
            get { return _attributeHierarchyVisible; }
            set { _attributeHierarchyVisible = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDimensionInstanceAttributeNode() { }
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
