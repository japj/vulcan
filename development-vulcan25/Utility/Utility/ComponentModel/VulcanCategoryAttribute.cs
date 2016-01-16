using System;
using System.ComponentModel;

namespace Vulcan.Utility.ComponentModel
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class VulcanCategoryAttribute : CategoryAttribute
    {
        public VulcanCategoryAttribute()
            : base()
        {
        }

        public VulcanCategoryAttribute(string category)
            : base(category)
        {
        }
    }
}
