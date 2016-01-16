using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Markup;
using AstFramework.Model;
using AstFramework.Validation;
using Vulcan.Utility.Common;
using Vulcan.Utility.ComponentModel;
using Vulcan.Utility.Xml;
using VulcanEngine.AstFramework;

namespace VulcanEngine.IR.Ast
{
    public partial class AstNode : IDataErrorInfo
    {
        private IAstValidator _astValidator;

        [Browsable(false)]
        internal IAstValidator AstValidator
        {
            get { return _astValidator; }
            set { _astValidator = value; }
        }

        [BrowsableAttribute(false)]
        public string Error
        {
            get
            {
                if (_astValidator != null)
                {
                    VulcanValidationItem validationItem = _astValidator.ValidationItems.FirstOrDefault(item => item.InvalidItem.Equals(this));
                    if (validationItem != null)
                    {
                        return String.Format(CultureInfo.InvariantCulture, "Message: {0}\nRecommendation: {1}", validationItem.Message, validationItem.Recommendation);
                    }
                }

                return null;
            }
        }

        [BrowsableAttribute(false)]
        public string this[string columnName]
        {
            get
            {
                VulcanValidationItem validationItem = _astValidator.ValidationItems.FirstOrDefault(item => item.InvalidItem.Equals(this) && item.PropertyName.Equals(columnName));
                if (validationItem != null)
                {
                    return Enum.GetName(typeof(Severity), validationItem.Severity);
                }

                return null;
            }
        }

        protected AstNode(IFrameworkItem parentItem)
        {
            InitializeAstNode();
            _astValidator = AstValidatorProvider.AstValidator;

            // Create Placeholder XObject Definition Mapping
            _boundXObject = new XObjectMapping(new XElement(XName.Get("Placeholder"), null, null), null);
            SingletonPropertyChanged += AstNode_AnySingletonPropertyChanged;

            ParentItem = parentItem;
        }

        [BrowsableAttribute(false)]
        [AstUndoIneligibleProperty]
        public bool IsAstNodeReadOnly
        {
            get { return BimlFile != null && !BimlFile.IsParseable; }
        }

        private void AstNode_AnySingletonPropertyChanged(object sender, VulcanPropertyChangedEventArgs e)
        {
            PropertyInfo property = GetType().GetProperty(e.PropertyName);

            if (property != null)
            {
                if (e.PropertyName == "BimlFile")
                {
                    var oldBimlFile = e.OldValue as BimlFile;
                    var newBimlFile = e.NewValue as BimlFile;
                    if (oldBimlFile != null)
                    {
                        oldBimlFile.ParseableChanged += BimlFile_ParseableChanged;
                    }

                    if (newBimlFile != null)
                    {
                        newBimlFile.ParseableChanged += BimlFile_ParseableChanged;
                    }
                }
            }
        }

        private void BimlFile_ParseableChanged(object sender, EventArgs e)
        {
            VulcanOnPropertyChanged("IsAstNodeReadOnly", !IsAstNodeReadOnly, IsAstNodeReadOnly);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced Ast navigation method for advanced developers.")]
        public TChild FirstChildOfType<TChild>()
        {
            foreach (var property in GetType().GetProperties())
            {
                if (typeof(TChild).IsAssignableFrom(property.PropertyType))
                {
                    var firstChild = property.GetValue(this, null);
                    if (firstChild is TChild)
                    {
                        return (TChild)firstChild;
                    }
                }
            }

            return default(TChild);
        }
    }
}
