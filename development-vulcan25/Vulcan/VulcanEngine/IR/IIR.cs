using System;

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
