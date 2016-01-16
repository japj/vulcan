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
    public class AstDimensionInstanceHierarchyNode : AstNode
    {
        #region Private Storage
        private AstDimensionHierarchyNode _dimensionHierarchy;
        private bool _visible;
        private bool _enabled;
        private SsasOptimizedState _optimizedState;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("DimensionHierarchyName", "http://tempuri.org/vulcan2.xsd")]
        public AstDimensionHierarchyNode DimensionHierarchy
        {
            get { return _dimensionHierarchy; }
            set { _dimensionHierarchy = value; }
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public SsasOptimizedState OptimizedState
        {
            get { return _optimizedState; }
            set { _optimizedState = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDimensionInstanceHierarchyNode() { }
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
