using System;
using AstFramework.Model;
using Vulcan.Utility.ComponentModel;

namespace VulcanEngine.IR.Ast.Fact
{
    public partial class AstMeasureAggregateColumnNode : IVulcanEditableObject
    {
        public AstMeasureAggregateColumnNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        #region IEditableObject Support
        private string cachedName = String.Empty;

        public void BeginEdit()
        {
            // Save name before entering edit mode.
            cachedName = Column.Name;
        }

        public void CancelEdit()
        {
            if (cachedName != null)
            {
                Column.Name = cachedName;
            }

            cachedName = String.Empty;
        }

        public void EndEdit()
        {
            cachedName = String.Empty;
        }
        #endregion // IEditableObject Support
    }
}
