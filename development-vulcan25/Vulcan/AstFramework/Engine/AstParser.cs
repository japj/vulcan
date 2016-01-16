using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Xml.Linq;
using AstFramework.Engine.Binding;
using AstFramework.Markup;
using AstFramework.Model;
using AstFramework.Properties;
using Vulcan.Utility.Common;
using Vulcan.Utility.Xml;

namespace AstFramework.Engine
{
    public static class AstParser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need general exception handling.  No risk of bad state.")]
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static UnboundReferences ParseBimlFiles(IEnumerable<BimlFile> bimlFiles, ISymbolTableProvider astRootNode, LanguageSettings languageSettings)
        {
            var unboundReferences = new UnboundReferences();
            foreach (BimlFile bimlFile in bimlFiles)
            {
                var xdocument = bimlFile.XDocument;
                if (xdocument != null && xdocument.Root != null)
                {
                    try
                    {
                        SideEffectManager.SideEffectMode = AstSideEffectMode.NoSideEffects;
                        ParseDocument(bimlFile, astRootNode, unboundReferences, languageSettings);
                        SideEffectManager.SideEffectMode = AstSideEffectMode.ConsistencySideEffects;

                        bimlFile.IsParseable = true;
                    }
                    catch (Exception)
                    {
                        bimlFile.IsParseable = false;
                    }
                }
            }

            unboundReferences.ResolveAll(astRootNode.SymbolTable);
            return unboundReferences;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Derived type is used for static enforcement of contract.")]
        public static UnboundReferences ParseDocument(BimlFile bimlFile, IFrameworkItem frameworkItem, UnboundReferences unboundReferences, LanguageSettings languageSettings)
        {
            RecursiveParseElement(new ParserContext(bimlFile.XDocument.Root, frameworkItem, null, bimlFile, unboundReferences, languageSettings));
            return unboundReferences;
        }

        public static UnboundReferences ParseElement(XElement element, IFrameworkItem frameworkItem, BimlFile bimlFile, UnboundReferences unboundReferences, LanguageSettings languageSettings)
        {
            frameworkItem.BoundXObject = new XObjectMapping(element, null);
            RecursiveParseElement(new ParserContext(element, frameworkItem, null, bimlFile, unboundReferences, languageSettings));
            return unboundReferences;
        }

        private static void RecursiveParseElement(ParserContext context)
        {
            // TODO: Can we eliminate this special case?
            if (context.FrameworkItem is ISymbolTableProvider)
            {
                context.FrameworkItem.BoundXObject = new XObjectMapping(context.XElement, null);
            }

            var template = context.FrameworkItem as ITemplate;
            if (template != null)
            {
                context = new ParserContext(context.XElement, context.FrameworkItem, template, context.BimlFile, context.UnboundReferences, context.LanguageSettings);
            }

            ParseAttributes(context);
            ParseChildElements(context);
        }

        private static void ParseAttributes(ParserContext context)
        {
            foreach (XAttribute attribute in context.XElement.Attributes())
            {
                PropertyBindingAttributePair propertyBinding = PropertyBinder.GetPropertyBinding(context, attribute.Name, true);
                ParseChildXObject(context, attribute, attribute.Value, propertyBinding == null ? null : propertyBinding.Property);
            }
        }

        private static void ParseChildElements(ParserContext context)
        {
            foreach (XElement childElement in context.XElement.Elements())
            {
                PropertyBindingAttributePair propertyBinding = PropertyBinder.GetPropertyBinding(context, childElement.Name, false);
                ParseChildXObject(context, childElement, childElement.Value, propertyBinding == null ? null : propertyBinding.Property);
            }

            XElement element = context.XElement;
            PropertyBindingAttributePair textPropertyBinding = PropertyBinder.GetPropertyBinding(context, XName.Get("__self", element.Name.NamespaceName), false);
            AstXNameBindingAttribute textBindingAttribute = textPropertyBinding == null ? null : textPropertyBinding.BindingAttribute;
            if (textBindingAttribute != null && textBindingAttribute.IsSelf)
            {
                ParseChildXObject(context, element.LastNode, element.Value, textPropertyBinding.Property);
            }
        }

        private static void ParseChildXObject(ParserContext context, XObject xobject, string xmlValue, PropertyInfo propertyToBind)
        {
            if (propertyToBind != null)
            {
                var bindingAttributes = CustomAttributeProvider.Global.GetAttribute<AstXNameBindingAttribute>(propertyToBind, false);
                var xelement = xobject as XElement;
                if (CommonUtility.IsContainerOf(typeof(ICollection<object>), propertyToBind.PropertyType) && xelement != null && bindingAttributes[0].IsChildListElement)
                {
                    var mappingProvider = propertyToBind.GetValue(context.FrameworkItem, null) as IXObjectMappingProvider;
                    if (mappingProvider != null)
                    {
                        mappingProvider.BoundXObject = new XObjectMapping(xobject, propertyToBind.Name);
                    }

                    foreach (XElement childXElement in xelement.Elements())
                    {
                        ParseChildXObjectToElement(context, childXElement, childXElement.Value, propertyToBind);
                    }
                }
                else if (xobject != null)
                {
                    ParseChildXObjectToElement(context, xobject, xmlValue, propertyToBind);
                }
            }
        }

        private static void ParseChildXObjectToElement(ParserContext context, XObject xobject, string xmlValue, PropertyInfo propertyToBind)
        {
            if (!PropertyBinder.BindLiteralOrReference(context, xobject, xmlValue, propertyToBind))
            {
                var xelement = xobject as XElement;
                IFrameworkItem frameworkItem = CreateFrameworkItemInstance(context, xelement, propertyToBind);
                if (frameworkItem != null)
                {
                    PropertyBinder.BindFinalValue(propertyToBind, context.FrameworkItem, frameworkItem, xobject, true);
                    RecursiveParseElement(new ParserContext(xelement, frameworkItem, context.Template, context.BimlFile, context.UnboundReferences, context.LanguageSettings));
                    return;
                }
            }
        }

        private static IFrameworkItem CreateFrameworkItemInstance(ParserContext context, XElement element, PropertyInfo property)
        {
            ConstructorInfo boundFrameworkItemConstructor = null;
            IFrameworkItem boundItem = null;

            Type boundFrameworkItemType = Utility.GetFrameworkItemType(property, element.Name.LocalName);

            if (boundFrameworkItemType != null)
            {
                boundFrameworkItemConstructor = boundFrameworkItemType.GetConstructor(new[] { typeof(IFrameworkItem) });
            }

            if (boundFrameworkItemConstructor != null)
            {
                boundItem = (IFrameworkItem)boundFrameworkItemConstructor.Invoke(new[] { context.FrameworkItem });
                if (context.BimlFile != null)
                {
                    boundItem.BimlFile = context.BimlFile;

                    var emittableAstNode = boundItem as IEmittableAstNode;
                    if (emittableAstNode != null)
                    {
                        switch (context.BimlFile.EmitType)
                        {
                            case XmlIRDocumentType.Source:
                                emittableAstNode.Emit = true;
                                break;
                            case XmlIRDocumentType.Include:
                                emittableAstNode.Emit = false;
                                break;
                            default:
                                throw new NotImplementedException(String.Format(CultureInfo.InvariantCulture, Resources.ErrorUnknownDocType, context.BimlFile.EmitType));
                        }
                    }
                }
            }

            return boundItem;
        }
    }
}
