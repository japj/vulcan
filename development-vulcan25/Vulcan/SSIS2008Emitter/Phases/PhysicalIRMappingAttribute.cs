using System;

namespace Ssis2008Emitter.IR
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class PhysicalIRMappingAttribute : Attribute
    {
        public Type PhysicalIRType { get; private set; }

        public PhysicalIRMappingAttribute(Type physicalIRType)
        {
            PhysicalIRType = physicalIRType;
        }

        public override string ToString()
        {
            return PhysicalIRType.ToString();
        }

        public override bool Equals(object obj)
        {
            var attr = obj as PhysicalIRMappingAttribute;
            if (attr != null)
            {
                return PhysicalIRType.Equals(attr.PhysicalIRType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return PhysicalIRType.GetHashCode();
        }
    }
}
