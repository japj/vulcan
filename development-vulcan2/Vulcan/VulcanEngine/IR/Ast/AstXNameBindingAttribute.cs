using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VulcanEngine.IR.Ast
{
    // These are used exclusively for property binding by name
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class AstXNameBindingAttribute : System.Attribute
    {
        public AstXNameBindingAttribute(string localName, string namespaceName) : this(localName, namespaceName, null) { }

        public AstXNameBindingAttribute(string localName, string namespaceName, string XPathQuery)
        {
            this.XName = XName.Get(localName, namespaceName);
            this.XPathQuery = XPathQuery;
        }

        public override string ToString()
        {
            return XName.ToString();
        }

        public bool HasXPathQuery
        {
            get { return XPathQuery != null && !XPathQuery.Equals(String.Empty); }
        }

        public readonly XName XName;
        public readonly string XPathQuery;
    }
}
