using System;
using System.ComponentModel;

namespace Vulcan.Utility.ComponentModel
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "This is just a thin wrapper over DefaultValueAttribute, which does not expose the accessor.")]
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class VulcanDefaultValueAttribute : DefaultValueAttribute
    {
        public VulcanDefaultValueAttribute(bool value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(byte value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(char value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(double value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(float value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(int value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(long value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(object value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(short value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(string value) : base(value)
        {
        }

        public VulcanDefaultValueAttribute(Type type, string value) : base(type, value)
        {
        }
    }
}
