using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using AstFramework.Engine.Binding;
using AstFramework.Markup;
using AstFramework.Model;
using Vulcan.Utility.Common;

namespace AstFramework.Engine
{
    public class LanguageSettings
    {
        public string DefaultXmlNamespace { get; private set; }

        internal Dictionary<Type, Dictionary<XName, PropertyBindingAttributePair>> PropertyMappingDictionary { get; private set; }

        public LanguageSettings(string defaultXmlNamespace, Type rootItemType)
        {
            DefaultXmlNamespace = defaultXmlNamespace;
            PropertyMappingDictionary = new Dictionary<Type, Dictionary<XName, PropertyBindingAttributePair>>();
            PreLoadPropertyMappingsDictionary(rootItemType);
        }

        private void PreLoadPropertyMappingsDictionary(Type rootItemType)
        {
            foreach (Type itemType in Assembly.GetAssembly(rootItemType).GetExportedTypes())
            {
                if (typeof(IFrameworkItem).IsAssignableFrom(itemType) && !itemType.IsAbstract && !PropertyMappingDictionary.ContainsKey(itemType))
                {
                    var propertyBindings = new Dictionary<XName, PropertyBindingAttributePair>();
                    PropertyInfo[] itemProperties = itemType.GetProperties();

                    var bindingAttributeKeyValuePairs =
                        from property in itemProperties
                        from bindingAttribute in CustomAttributeProvider.Global.GetAttribute<AstXNameBindingAttribute>(property, false)
                        where property.CanWrite || CommonUtility.IsContainerOf(typeof(ICollection<object>), property.PropertyType)
                        select new { property, attribute = bindingAttribute };

                    foreach (var pair in bindingAttributeKeyValuePairs)
                    {
                        XName xname;
                        if (pair.attribute.IsChildListElement)
                        {
                            xname = XName.Get(pair.property.Name, DefaultXmlNamespace);
                        }
                        else
                        {
                            xname = XName.Get(pair.attribute.Name, DefaultXmlNamespace);
                        }

                        propertyBindings[xname] = new PropertyBindingAttributePair(pair.property, pair.attribute);
                    }

                    PropertyMappingDictionary.Add(itemType, propertyBindings);
                }
            }
        }
    }
}
