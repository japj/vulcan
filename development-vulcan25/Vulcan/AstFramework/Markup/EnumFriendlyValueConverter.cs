using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace AstFramework.Markup
{
    public class EnumFriendlyValueConverter : EnumConverter
    {
        private readonly IDictionary<object, string> _descriptionMap;

        public EnumFriendlyValueConverter(Type enumType) : base(enumType)
        {
            _descriptionMap = new Dictionary<object, string>();

            foreach (FieldInfo field in EnumType.GetFields())
            {
                foreach (object attribute in field.GetCustomAttributes(typeof(EnumFriendlyValueAttribute), false))
                {
                    _descriptionMap.Add(field.GetValue(null), ((EnumFriendlyValueAttribute)attribute).FriendlyValue);
                    break;
                }
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (EnumType.Equals(value.GetType()) && destinationType == typeof(string))
            {
                string str;

                if (_descriptionMap.TryGetValue(value, out str))
                {
                    return str;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                foreach (KeyValuePair<object, string> item in _descriptionMap)
                {
                    if (Equals(item.Value, value))
                    {
                        return item.Key;
                    }
                }

                return null;
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}