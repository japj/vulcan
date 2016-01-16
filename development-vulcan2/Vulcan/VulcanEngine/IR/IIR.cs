using System;
using System.Collections.Generic;
using System.Text;

namespace VulcanEngine.IR
{
    public interface IIR : ICloneable
    {
        string Name
        {
            get;
        }
    }
}
