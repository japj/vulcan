using System;
using System.Data.OleDb;

namespace Ssis2008Emitter.PlatformEmitters
{
    public static class OleDBTypeTranslator
    {
        public static OleDbType Translate(TypeCode type)
        {
            switch (type)
            {
                case TypeCode.Char: return OleDbType.WChar;
                case TypeCode.String: return OleDbType.WChar;
                case TypeCode.DateTime: return OleDbType.DBTimeStamp;
                case TypeCode.Decimal: return OleDbType.Decimal;
                case TypeCode.Double: return OleDbType.Double;
                case TypeCode.Byte: return OleDbType.UnsignedTinyInt;
                case TypeCode.Int16: return OleDbType.SmallInt;
                case TypeCode.Int32: return OleDbType.Integer;
                case TypeCode.Int64: return OleDbType.BigInt;
                case TypeCode.UInt16: return OleDbType.UnsignedSmallInt;
                case TypeCode.UInt32: return OleDbType.UnsignedInt;
                case TypeCode.UInt64: return OleDbType.UnsignedBigInt;
                case TypeCode.SByte: return OleDbType.TinyInt;
                case TypeCode.Single: return OleDbType.Single;
                case TypeCode.Boolean: return OleDbType.Boolean;
                case TypeCode.DBNull: return OleDbType.Empty;
                case TypeCode.Empty: return OleDbType.Empty;
                case TypeCode.Object: return OleDbType.Variant;
                default: throw new NotImplementedException("Unknown Data Type");
            }
        }
    }
}
