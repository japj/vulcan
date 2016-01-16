using System;
using System.Collections.Generic;
using System.Xml;
using Vulcan.Utility.Common;

namespace AstFramework.Engine.Binding
{
    public static class LiteralTypeConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Generic pattern not appropriate since type is discovered dynamically.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "Advanced method provided for advanced developers.")]
        public static bool TryConvert(Type type, string value, out object convertedValue)
        {
            if (CommonUtility.IsContainerOf(typeof(ICollection<object>), type))
            {
                type = type.GetGenericArguments()[0];
            }

            bool success;
            switch (type.FullName)
            {
                case "System.Boolean":
                    bool convertedBool;
                    success = Boolean.TryParse(value, out convertedBool);
                    convertedValue = convertedBool;
                    return success;
                case "System.Byte":
                    byte convertedByte;
                    success = Byte.TryParse(value, out convertedByte);
                    convertedValue = convertedByte;
                    return success;
                case "System.Char":
                    char convertedChar;
                    success = Char.TryParse(value, out convertedChar);
                    convertedValue = convertedChar;
                    return success;
                case "System.DateTime":
                    DateTime convertedDateTime;
                    success = DateTime.TryParse(value, out convertedDateTime);
                    convertedValue = convertedDateTime;
                    return success;
                case "System.Decimal":
                    decimal convertedDecimal;
                    success = Decimal.TryParse(value, out convertedDecimal);
                    convertedValue = convertedDecimal;
                    return success;
                case "System.Double":
                    double convertedDouble;
                    success = Double.TryParse(value, out convertedDouble);
                    convertedValue = convertedDouble;
                    return success;
                case "System.Int16":
                    short convertedShort;
                    success = Int16.TryParse(value, out convertedShort);
                    convertedValue = convertedShort;
                    return success;
                case "System.Int32":
                    int convertedInt;
                    success = Int32.TryParse(value, out convertedInt);
                    convertedValue = convertedInt;
                    return success;
                case "System.Int64":
                    long convertedLong;
                    success = Int64.TryParse(value, out convertedLong);
                    convertedValue = convertedLong;
                    return success;
                case "System.String":
                    convertedValue = value.Clone();
                    return true;
                case "System.TimeSpan":
                    convertedValue = XmlConvert.ToTimeSpan(value);
                    return true;
                case "System.Object":
                    bool convertedObjBool;
                    success = Boolean.TryParse(value, out convertedObjBool);
                    if (success)
                    {
                        convertedValue = convertedObjBool;
                        return true;
                    }

                    int convertedObjInt;
                    success = Int32.TryParse(value, out convertedObjInt);
                    if (success)
                    {
                        convertedValue = convertedObjInt;
                        return true;
                    }

                    break;
                default:
                    if (type.IsEnum)
                    {
                        // TODO: Add exception handling code with friendly error
                        try
                        {
                            convertedValue = Enum.Parse(type, value, true);
                            return true;
                        }
                        catch (ArgumentNullException)
                        {
                        }
                        catch (ArgumentException)
                        {
                            /* Message.Error("No converter available for that type */
                        }
                        catch (OverflowException)
                        {
                        }
                    }

                    break;
            }

            convertedValue = null;
            return false;
        }
    }
}
