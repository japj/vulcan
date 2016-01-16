using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Vulcan.Utility.Common
{
    public static class CommonUtility
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        public static bool HasProperty<TAttribute>(ICustomAttributeProvider memberInfo)
        {
            return memberInfo.GetCustomAttributes(typeof(TAttribute), false).Length > 0;
        }

        public static bool HasProperty(ICustomAttributeProvider memberInfo, Type attributeType)
        {
            return memberInfo.GetCustomAttributes(attributeType, false).Length > 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        public static HashSet<T> AllPropertyChildren<T>(object value)
        {
            HashSet<T> children = new HashSet<T>();
            if (value != null)
            {
                foreach (PropertyInfo propertyInfo in value.GetType().GetProperties())
                {
                    if (propertyInfo.GetIndexParameters().Length == 0)
                    {
                        ProcessProperty<T>(value, children, propertyInfo);
                    }
                }
            }

            return children;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        public static HashSet<T> AllPropertyChildren<T, TAttribute>(object value)
        {
            HashSet<T> children = new HashSet<T>();
            if (value != null)
            {
                foreach (PropertyInfo propertyInfo in value.GetType().GetProperties())
                {
                    if (HasProperty<TAttribute>(propertyInfo) && propertyInfo.GetIndexParameters().Length == 0)
                    {
                        ProcessProperty<T>(value, children, propertyInfo);
                    }
                }
            }

            return children;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need general exception handling.  No risk of bad state.")]
        private static void ProcessProperty<T>(object obj, HashSet<T> children, PropertyInfo propertyInfo)
        {
            object child = null;
            try
            {
                child = propertyInfo.GetValue(obj, null);
            }
            catch (Exception)
            {
                // Ignore exception on property access
            }

            if (child != null)
            {
                if (child is T && !children.Contains((T)child))
                {
                    children.Add((T)child);
                }
                else if (IsContainerOf(typeof(ICollection<T>), child.GetType()))
                {
                    foreach (T collectionChild in (ICollection)child)
                    {
                        if (!children.Contains(collectionChild))
                        {
                            children.Add(collectionChild);
                        }
                    }
                }
            }
        }

        #region Generics Helpers
        public static bool IsContainerOf(Type containerBaseType, Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            Type[] baseArgs = containerBaseType.GetGenericArguments();
            Type[] checkArgs = type.GetGenericArguments();

            if (baseArgs.Length != checkArgs.Length)
            {
                return false;
            }

            for (int i = 0; i < baseArgs.Length; i++)
            {
                if (!baseArgs[i].IsAssignableFrom(checkArgs[i]))
                {
                    return false;
                }
            }

            // We've established that the type parameter lists are compatible, so now we check to see if the container type is compatible
            // Due to the way .NET reflection handles generic types, we need to construct a candidate type with the contained parameter list
            return containerBaseType.IsAssignableFrom(type.GetGenericTypeDefinition().MakeGenericType(baseArgs));
        }
        #endregion Generics Helpers

        #region System.Xml Helpers
        public static XmlNode[] TextToNodeArray(string text)
        {
            XmlDocument doc = new XmlDocument();
            return new XmlNode[1] { doc.CreateTextNode(text) };
        }
        #endregion System.Xml Helpers

        #region System.IO Helpers
        public static Collection<string> FileListFromInputPaths(Collection<string> inputPaths)
        {
            Collection<string> results = new Collection<string>();

            foreach (string inputPath in inputPaths)
            {
                if (Directory.Exists(inputPath))
                {
                    try
                    {
                        foreach (string filename in Directory.GetFiles(inputPath, "*.xml", SearchOption.AllDirectories))
                        {
                            results.Add(filename);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // TODO: Do anything in the catch block?
                    }
                }
                else
                {
                    results.Add(inputPath);
                }
            }

            return results;
        }
        #endregion System.IO Helpers
    }
}
