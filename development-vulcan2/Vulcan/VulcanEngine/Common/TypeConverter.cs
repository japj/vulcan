using System;
using System.Collections.Generic;
using System.Text;

using VulcanEngine.Properties;
using VulcanEngine.Common;

namespace VulcanEngine.Common
{
    // REVIEW: Pluggable type converters would be very easy with this model - Is this someting we need right now or should we add to a future feature list?
    class TypeConverter
    {
        public static object Convert(object Input, Type OutputType)
        {
            if (OutputType.IsAssignableFrom(Input.GetType()))
            {
                return Input;
            }
            MessageEngine.Global.Trace(Severity.Error, Resources.ErrorRequiredTypeConverterDoesNotExist, OutputType.AssemblyQualifiedName);
            return null;
        }
    }
}
