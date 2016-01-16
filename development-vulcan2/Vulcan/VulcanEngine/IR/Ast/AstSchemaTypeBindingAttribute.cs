using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace VulcanEngine.IR.Ast
{
    // These are used exclusively for determining which type to use during object construction.
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class AstSchemaTypeBindingAttribute : System.Attribute
    {
        public AstSchemaTypeBindingAttribute(string localName, string namespaceName)
        {
            this.LocalName = localName;
            this.NamespaceName = namespaceName;
            this.SchemaTypeName = String.Format("{{{0}}}{1}", namespaceName, localName);
            this.XmlQualifiedName = new XmlQualifiedName(localName, namespaceName);
        }

        public override string ToString()
        {
            return SchemaTypeName;
        }

        public readonly string SchemaTypeName;
        public readonly string LocalName;
        public readonly string NamespaceName;
        public readonly XmlQualifiedName XmlQualifiedName;
    }
}
