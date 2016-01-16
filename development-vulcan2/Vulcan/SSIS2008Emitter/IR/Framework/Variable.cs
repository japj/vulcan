using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;
using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.Framework
{
    public class Variable : Task.Task
    {
        #region Private Storage
        private string _type;
        private Object _value;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public Object Value
        {
            get { return _value; }
            set { _value = ParseValue((string)value); }
        }

        private Object ParseValue(string sValue)
        {
            Object ret = null;

            try
            {
                switch (Type.ToUpperInvariant())
                {
                    case "STRING": ret = sValue; break;
                    case "INT32": ret = Int32.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "INT64": ret = Int64.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "BOOLEAN": ret = Boolean.Parse(sValue); break;
                    case "OBJECT": ret = new Object(); break;
                    case "BYTE": ret = Byte.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "CHAR": ret = Char.Parse(sValue); break;
                    case "DATETIME": ret = DateTime.Parse(sValue); break;
                    case "DBNULL": ret = DBNull.Value; break;
                    case "DOUBLE": ret = Double.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "INT16": ret = Int16.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "SBYTE": ret = SByte.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "SINGLE": ret = Single.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "UINT32": ret = UInt32.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "UINT64": ret = UInt64.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture); break;
                        
                    default: MessageEngine.Global.Trace(Severity.Error, "Failure parsing package variables: Variable {0}, Unknown type {1}", Name, Type); break;
                }
            }
            catch (Exception e)
            {
                MessageEngine.Global.Trace(Severity.Error, e, "Failure parsing package variables: Variable {0}, Type {1}, Value {2}", Name, Type, sValue);
            }

            return ret;
        }
        #endregion  // Public Accessor Properties
    }
}
