using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Schema;

namespace AstFramework.Markup
{
    public class AttributeUseConverter : EnumConverter
    {
        public AttributeUseConverter(Type enumType) : base(enumType)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is ChildUse && destinationType == typeof(XmlSchemaUse))
            {
                switch ((ChildUse)value)
                {
                    case ChildUse.Optional: return XmlSchemaUse.Optional;
                    case ChildUse.Required: return XmlSchemaUse.Required;
                    default: throw new NotSupportedException("Unconvertable VulcanXmlSchemaAttributeUse");
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is XmlSchemaUse)
            {
                switch ((XmlSchemaUse)value)
                {
                    case XmlSchemaUse.Optional: return ChildUse.Optional;
                    case XmlSchemaUse.Required: return ChildUse.Required;
                    default: throw new NotSupportedException("Unconvertable VulcanXmlSchemaAttributeUse");
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}