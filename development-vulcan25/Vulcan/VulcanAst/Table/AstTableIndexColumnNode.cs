using System;
using AstFramework.Model;
using Vulcan.Utility.ComponentModel;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableIndexColumnNode : IVulcanEditableObject
    {
        public AstTableIndexColumnNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        public static bool StructureEquals(AstTableIndexColumnNode indexColumn1, AstTableIndexColumnNode indexColumn2)
        {
            if (indexColumn1 == null || indexColumn2 == null)
            {
                return indexColumn1 == null && indexColumn2 == null;
            }

            bool match = true;
            match &= indexColumn1.Column == indexColumn2.Column;
            match &= indexColumn1.SortOrder == indexColumn2.SortOrder;
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
