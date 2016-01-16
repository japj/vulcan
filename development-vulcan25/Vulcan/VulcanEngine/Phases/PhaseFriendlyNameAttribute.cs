using System;

namespace VulcanEngine.Phases
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class PhaseFriendlyNameAttribute : Attribute
    {
        public PhaseFriendlyNameAttribute(string phaseFriendlyName)
        {
            PhaseFriendlyName = phaseFriendlyName;
        }

        public override string ToString()
        {
            return PhaseFriendlyName;
        }

        public string PhaseFriendlyName { get; private set; }
    }
}
