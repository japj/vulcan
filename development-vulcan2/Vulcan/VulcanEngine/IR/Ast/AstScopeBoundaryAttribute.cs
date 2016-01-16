using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class AstScopeBoundaryAttribute : System.Attribute
    {
        public AstScopeBoundaryAttribute()
        {
        }

    }
}
