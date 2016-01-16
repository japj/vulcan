using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VulcanEngine.IR.Ast
{
    public class AstBindingAttribute : System.Attribute
    {
        public AstBindingAttribute(string localName, string namespaceName)
        {
            XName = XName.Get(localName, namespaceName);
        }

        public override string ToString()
        {
            return XName.ToString();
        }

        public readonly XName XName;    }
}
