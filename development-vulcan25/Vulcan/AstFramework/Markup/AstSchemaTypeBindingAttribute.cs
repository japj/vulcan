using System;
using System.Globalization;
using System.Xml;

namespace AstFramework.Markup
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class AstSchemaTypeBindingAttribute : Attribute
    {
        public string SchemaTypeName { get; private set; }

        public string LocalName { get; private set; }

        public string NamespaceName { get; private set; }

        public XmlQualifiedName XmlQualifiedName { get; private set; }

        public AstSchemaTypeBindingAttribute(string localName, string namespaceName)
        {
            LocalName = localName;
            NamespaceName = namespaceName;
            SchemaTypeName = String.Format(CultureInfo.InvariantCulture, "{{{0}}}{1}", namespaceName, localName);
            XmlQualifiedName = new XmlQualifiedName(localName, namespaceName);
        }

        public override string ToString()
        {
            return SchemaTypeName;
        }
    }
}