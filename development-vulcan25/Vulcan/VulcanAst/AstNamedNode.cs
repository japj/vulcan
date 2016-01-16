using System;
using System.ComponentModel;
using System.Globalization;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using Vulcan.Utility.ComponentModel;
using VulcanEngine.AstFramework;

namespace VulcanEngine.IR.Ast
{
    public partial class AstNamedNode : IVulcanEditableObject
    {
        public AstNamedNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        public override string ToString()
        {
            return Name;
        }

        [BrowsableAttribute(false)]
        public string ScopedName
        {
            get { return BuildScopedName(Name); }
        }
        
        [BrowsableAttribute(false)]
        public SymbolTable SymbolTable
        {
            get { return SymbolTableProvider.SymbolTable; }
        }

        public string BuildScopedName(string name)
        {
            var scopeAncestor = ScopeBoundary;
            if (scopeAncestor != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", scopeAncestor.ScopedName, name);
            }

            return name;
        }

        #region IEditableObject Support
        private string _cachedName = String.Empty;
        private bool _editMode;

        public void BeginEdit()
        {
            // Save name before entering edit mode.
            _cachedName = Name;
            IsInEditMode = true;
        }

        public void CancelEdit()
        {
            if (_cachedName != null)
            {
                Name = _cachedName;
            }

            _cachedName = String.Empty;
            IsInEditMode = false;
        }

        public void EndEdit()
        {
            _cachedName = String.Empty;
            IsInEditMode = false;
        }

        [Browsable(false)]
        [AstUndoIneligibleProperty]
        public bool IsInEditMode
        {
            get { return _editMode; }
            private set
            {
                if (_editMode != value)
                {
                    _editMode = value;
                    VulcanOnPropertyChanged("IsInEditMode", !_editMode, _editMode);
                }
            }
        }
        #endregion // IEditableObject Support
    }
}
