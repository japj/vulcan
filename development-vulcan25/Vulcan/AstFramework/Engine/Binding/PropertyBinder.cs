using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using AstFramework.Engine.Expressions;
using AstFramework.Model;
using Vulcan.Utility.Common;
using Vulcan.Utility.Xml;

namespace AstFramework.Engine.Binding
{
    public static class PropertyBinder
    {
        public static bool BindLiteralOrReference(ParserContext context, XObject xmlObject, string xmlValue, PropertyInfo boundProperty)
        {
            object convertedLiteralValue;

            if (LiteralTypeConverter.TryConvert(boundProperty.PropertyType, xmlValue, out convertedLiteralValue))
            {
                if (BindExpression(context, xmlObject, xmlValue, boundProperty))
                {
                    return true;
                }
             
                BindFinalValue(boundProperty, context.FrameworkItem, convertedLiteralValue, xmlObject, true);
                return true;
            }

            if (xmlObject is XAttribute)
            {
                if (BindExpression(context, xmlObject, xmlValue, boundProperty))
                {
                    return true;
                }

                DelayedBind(context, xmlObject, xmlValue, boundProperty);
                return true;
            }

            return false;
        }

        private static bool BindExpression(ParserContext context, XObject xmlObject, string xmlValue, PropertyInfo boundProperty)
        {
            var tokenSequence = new TokenSequence(xmlValue);
            if (tokenSequence.RequiresProcessing)
            {
                DelayedBind(context, xmlObject, xmlValue, boundProperty);
                return true;
            }

            return false;
        }

        private static void DelayedBind(ParserContext context, XObject xmlObject, string xmlValue, PropertyInfo boundProperty)
        {
            IFrameworkItem frameworkItem = context.FrameworkItem;
            var bindingItem = new BindingItem(boundProperty, xmlObject, xmlValue, frameworkItem, context.BimlFile, null);
            if (context.IsInTemplate)
            {
                context.Template.UnboundReferences.Add(bindingItem);
            }
            else
            {
                context.UnboundReferences.Add(bindingItem);
            }
        }

        public static void BindFinalValue(PropertyInfo property, object parentItem, object value, XObject sourceXObject, bool definition)
        {
            sourceXObject.AddAnnotation(value);
            Type propertyType = property.PropertyType;

            if (CommonUtility.IsContainerOf(typeof(ICollection<object>), propertyType) && propertyType.GetGenericArguments()[0].IsAssignableFrom(value.GetType()))
            {
                object collection = property.GetValue(parentItem, null);
                MethodInfo addMethod = collection.GetType().GetMethod("Add");
                addMethod.Invoke(collection, new[] { value });
            }
            else if (propertyType.IsAssignableFrom(value.GetType()))
            {
                property.SetValue(parentItem, value, null);
            }
            else
            {
                // TODO: Message.Error("No Binding Mechanism Worked");
            }

            var mappingProvider = value as IXObjectMappingProvider;
            if (mappingProvider != null && definition)
            {
                mappingProvider.BoundXObject = new XObjectMapping(sourceXObject, property.Name);
            }
        }

        public static PropertyBindingAttributePair GetPropertyBinding(ParserContext context, XName xmlObjectName, bool patchDefaultNamespace)
        {
            Dictionary<XName, PropertyBindingAttributePair> propertyBinding;
            if (context.LanguageSettings.PropertyMappingDictionary.TryGetValue(context.FrameworkItem.GetType(), out propertyBinding))
            {
                if (propertyBinding.ContainsKey(xmlObjectName))
                {
                    return propertyBinding[xmlObjectName];
                }

                if (patchDefaultNamespace && String.IsNullOrEmpty(xmlObjectName.NamespaceName))
                {
                    // targetNamespace is not applied to attributes, so this code will use the DefaultXMLNamespace to perform XName matching when specified (i.e. for attributes)
                    return GetPropertyBinding(context, XName.Get(xmlObjectName.LocalName, context.LanguageSettings.DefaultXmlNamespace), false);
                }
            }

            return null;
        }
    }
}
