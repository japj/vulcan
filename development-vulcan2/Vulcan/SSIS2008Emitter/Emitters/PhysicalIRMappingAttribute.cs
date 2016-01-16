using System;
using System.Collections.Generic;
using System.Text;

namespace Ssis2008Emitter.Emitters
{
    public class PhysicalIRMappingAttribute : System.Attribute
    {
        public PhysicalIRMappingAttribute(Type physicalIRType)
        {
            this.PhysicalIRType = physicalIRType;
        }

        public override string ToString()
        {
            return PhysicalIRType.ToString();
        }

        public override bool Equals(object obj)
        {
            PhysicalIRMappingAttribute attr = obj as PhysicalIRMappingAttribute;
            if (attr != null)
            {
                return this.PhysicalIRType.Equals(attr.PhysicalIRType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return PhysicalIRType.GetHashCode();
        }

        public readonly Type PhysicalIRType;
    }
}
