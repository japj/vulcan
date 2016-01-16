using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VulcanEngine.IR.Ast
{
    // These are used exclusively for providing a custom default namespace on a ASTNode type
    public class AstDefaultXmlNamespaceBindingAttribute : System.Attribute
    {
        public AstDefaultXmlNamespaceBindingAttribute(string namespaceName)
        {
            this.NamespaceName = namespaceName;
        }

        public override string ToString()
        {
            return NamespaceName;
        }

        public readonly string NamespaceName;
    }
}
