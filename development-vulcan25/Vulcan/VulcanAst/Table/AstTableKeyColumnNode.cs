using System;
using AstFramework.Model;
using Vulcan.Utility.ComponentModel;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableKeyColumnNode : IVulcanEditableObject
    {
        public AstTableKeyColumnNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        public static bool StructureEquals(AstTableKeyColumnNode keyColumn1, AstTableKeyColumnNode keyColumn2)
        {
            if (keyColumn1 == null || keyColumn2 == null)
            {
                return keyColumn1 == null && keyColumn2 == null;
            }

            bool match = true;
            match &= keyColumn1.SortOrder == keyColumn2.SortOrder;
            match &= keyColumn1.Column == keyColumn2.Column;
            return match;
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
