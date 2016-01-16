using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using AstFramework.Markup;
using Vulcan.Utility.Common;

namespace AstFramework.Engine
{
    public static class Utility
    {
        public static AstXNameBindingAttribute FindBindingAttributeForSubtype(MemberInfo memberInfo, Type subtype)
        {
            var bindingAttributes = CustomAttributeProvider.Global.GetAttribute<AstXNameBindingAttribute>(memberInfo, false);

            AstXNameBindingAttribute lastMatched = null;
            foreach (AstXNameBindingAttribute bindingAttribute in bindingAttributes)
            {
                if (!bindingAttribute.HasSubtypeOverride || bindingAttribute.SubtypeOverride.IsAssignableFrom(subtype))
                {
                    lastMatched = bindingAttribute;
                }
            }

            return lastMatched;
        }

        public static IEnumerable<string> FileListFromInputPaths(IEnumerable<string> inputPaths)
        {
            var results = new List<string>();

            foreach (string inputPath in inputPaths)
            {
                /*
                if (Directory.Exists(inputPath))
                {
                    try
                    {
                        results.AddRange(Directory.GetFiles(inputPath, "*.xml", SearchOption.AllDirectories));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // TODO: Do anything in the catch block?
                    }
                }
                else
                {
                */
                if (!Directory.Exists(inputPath))
                {
                    results.Add(inputPath);
                }
                ////}
            }

            return results;
        }

        public static Type GetFrameworkItemType(PropertyInfo property, string name)
        {
            Type boundFrameworkItemType = property.PropertyType;
            if (CommonUtility.IsContainerOf(typeof(ICollection<object>), property.PropertyType))
            {
                boundFrameworkItemType = boundFrameworkItemType.GetGenericArguments()[0];
            }

            var bindingAttributes = CustomAttributeProvider.Global.GetAttribute<AstXNameBindingAttribute>(property, false);
            foreach (AstXNameBindingAttribute bindingAttribute in bindingAttributes)
            {
                if (bindingAttribute.HasSubtypeOverride && name.Equals(bindingAttribute.Name))
                {
                    boundFrameworkItemType = bindingAttribute.SubtypeOverride;
                }
            }

            return boundFrameworkItemType;
        }

        public static XmlNode[] TextToNodeArray(string text)
        {
            XmlDocument doc = new XmlDocument();
            return new XmlNode[1] { doc.CreateTextNode(text) };
        }

        public static bool IsNullOrEmpty(object item)
        {
            if (item == null)
            {
                return true;
            }

            string itemAsString = item as string;
            if (itemAsString != null)
            {
                return String.IsNullOrEmpty(itemAsString);
            }

            return false;
        }
    }
}
