using System;
using System.Collections.Generic;
using System.Text;

namespace VulcanEngine.Phases
{
    public class PhaseFriendlyNameAttribute : System.Attribute
    {
        public PhaseFriendlyNameAttribute(string PhaseFriendlyName)
        {
            this.PhaseFriendlyName = PhaseFriendlyName;
        }

        public override string ToString()
        {
            return PhaseFriendlyName;
        }

        public readonly string PhaseFriendlyName;
    }
}
