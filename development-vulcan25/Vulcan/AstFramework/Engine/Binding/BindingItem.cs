using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Xml.Linq;
using AstFramework.Engine.Expressions;
using AstFramework.Markup;
using AstFramework.Model;
using Vulcan.Utility.Common;

namespace AstFramework.Engine.Binding
{
    public class BindingItem
    {
        public PropertyInfo BoundProperty { get; private set; }

        public XObject XObject { get; private set; }

        public string XValue { get; private set; }

        public IFrameworkItem ParentItem { get; private set; }

        public BimlFile BimlFile { get; private set; }

        public ITemplateInstance TemplateInstance { get; private set; }

        public BindingItem(PropertyInfo boundProperty, XObject xmlObject, string xmlValue, IFrameworkItem parentItem, BimlFile bimlFile, ITemplateInstance templateInstance)
        {
            BoundProperty = boundProperty;
            XObject = xmlObject;
            XValue = xmlValue;
            ParentItem = parentItem;
            BimlFile = bimlFile;
            TemplateInstance = templateInstance;
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public bool Bind(SymbolTable symbolTable)
        {
            if (!ProcessExpression(symbolTable))
            {
                return false;
            }

            object convertedLiteralValue;
            if (LiteralTypeConverter.TryConvert(BoundProperty.PropertyType, XValue, out convertedLiteralValue))
            {
                PropertyBinder.BindFinalValue(BoundProperty, ParentItem, convertedLiteralValue, XObject, false);
                return true;
            }

            IFrameworkItem boundItem = ScopedSearch(XValue, symbolTable);
            if (boundItem != null)
            {
                PropertyBinder.BindFinalValue(BoundProperty, ParentItem, boundItem, XObject, false);
                return true;
            }

            return false;
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private bool ProcessExpression(SymbolTable symbolTable)
        {
            // TODO: If there are performance problems, we could find a faster way to detect expressions than doing a full parse
            var tokenSequence = new TokenSequence(XValue);

            if (tokenSequence.RequiresProcessing)
            {
                if (tokenSequence.RequiresTemplateArguments)
                {
                    var templateInstance = TemplateInstance;
                    if (templateInstance == null || templateInstance.Template == null)
                    {
                        return false;
                    }

                    XValue = tokenSequence.Process(symbolTable, templateInstance.ArgumentDictionary);
                }
                else
                {
                    XValue = tokenSequence.Process(symbolTable, null);
                }
            }

            return true;
        }

        private IFrameworkItem ScopedSearch(string xmlValue, SymbolTable symbolTable)
        {
            Type t = BoundProperty.PropertyType;
            if (CommonUtility.IsContainerOf(typeof(ICollection<object>), t))
            {
                t = t.GetGenericArguments()[0];
            }

            var attributes = CustomAttributeProvider.Global.GetAttribute<AstLocalOnlyScopeBindingAttribute>(BoundProperty, false);
            bool localOnlyScopeBinding = attributes.Count > 0;

            IReferenceableItem boundItem;
            IEnumerable<IScopeBoundary> bindingScopeBoundaries = ParentItem.BindingScopeBoundaries();
            foreach (var scopeBoundary in bindingScopeBoundaries)
            {
                IScopeBoundary currentScope = scopeBoundary;
                while (currentScope != null)
                {
                    boundItem = FindReferenceableItem(symbolTable, currentScope, t, xmlValue);
                    currentScope = currentScope.ScopeBoundary;
                    if (CheckBoundItem(boundItem, localOnlyScopeBinding, bindingScopeBoundaries))
                    {
                        return boundItem;
                    }
                }
            }

            // Attempt scopeless binding
            boundItem = FindReferenceableItem(symbolTable, null, t, xmlValue);
            if (CheckBoundItem(boundItem, localOnlyScopeBinding, bindingScopeBoundaries))
            {
                return boundItem;
            }

            return null;
        }

        private static IReferenceableItem FindReferenceableItem(SymbolTable symbolTable, IScopeBoundary scopeBoundary, Type itemType, string symbolName)
        {
            string fullName = symbolName;
            if (scopeBoundary != null)
            {
                fullName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", scopeBoundary.ScopedName, symbolName);
            }

            return symbolTable[itemType, fullName];
        }

        private static bool CheckBoundItem(IReferenceableItem boundItem, bool localOnlyScopeBinding, IEnumerable<IScopeBoundary> scopeBoundaries)
        {
            if (boundItem != null)
            {
                if (localOnlyScopeBinding)
                {
                    foreach (var scopeBoundary in scopeBoundaries)
                    {
                        if (boundItem.ScopeBoundary == scopeBoundary)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                return true;
            }

            return false;
        }
    }
}