using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Vulcan.Utility.ComponentModel
{
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class VulcanDescriptionAttribute : DescriptionAttribute
    {
        public VulcanDescriptionAttribute()
            : base()
        {
        }

        public VulcanDescriptionAttribute(string description)
            : base(description)
        {
        }
    }
}
