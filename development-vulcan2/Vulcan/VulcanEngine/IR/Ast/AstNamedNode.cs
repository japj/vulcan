using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast
{
    public abstract class AstNamedNode : AstNode, IVulcanEditableObject
    {
        private string _name;
        private bool _asClassOnly;

        public override string ReferenceableName
        {
            get
            {
                if (ParentASTNode != null && ParentASTNode.ReferenceableName != null)
                {
                    return ParentASTNode.ReferenceableName + Common.IRUtility.NamespaceSeparator + this.Name;
                }
                else
                {
                    return this.Name;
                }
            }
        }
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                VulcanOnPropertyChanged("Name");
            }
        }

        public bool AsClassOnly
        {
            get
            {
                return _asClassOnly;
            }
            set
            {
                _asClassOnly = value;
            }
        }

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            return validationItems;
        }
        #endregion  // Validation

        #region IEditableObject Support

        private string cachedName = String.Empty;

        public void BeginEdit()
        {
            // Save name before entering edit mode.
            cachedName = this.Name;
        }

        public void CancelEdit()
        {
            if (cachedName != null)
            {
                Name = cachedName;
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
