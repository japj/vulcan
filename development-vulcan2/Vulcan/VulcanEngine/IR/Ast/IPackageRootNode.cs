using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast
{
    public interface IPackageRootNode
    {
        bool Emit
        {
            get;
            set;
        }
    }
}
