using System;
using System.Globalization;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Table;


namespace AstLowerer.TSqlEmitter
{
    public static class TSqlTypeTranslator
    {
        public static string Translate(ColumnType type, int length, int precision, int scale, string customType)
        {
            length = length >= 1 ? length : 1;
            // if customType is set, don't try and build a data type, accept what the user gives.
            if(!String.IsNullOrEmpty(customType))
            {
                return customType;
            }
            switch (type)
            {
                case ColumnType.AnsiString: return String.Format(CultureInfo.InvariantCulture, "varchar({0})", length <= 8000 ? length.ToString(CultureInfo.InvariantCulture) : "max");
                case ColumnType.Binary: return String.Format(CultureInfo.InvariantCulture, "varbinary({0})", length <= 8000 ? length.ToString(CultureInfo.InvariantCulture) : "max");
                case ColumnType.Byte: return "tinyint";
                case ColumnType.Boolean: return "bit";
                case ColumnType.Currency: return "money";
                case ColumnType.Date: return "date";
                case ColumnType.DateTime: return "datetime";

                case ColumnType.VarNumeric:
                case ColumnType.Decimal: return String.Format(CultureInfo.InvariantCulture, "decimal({0},{1})", precision >= 1 ? precision : 18, scale >= 0 ? scale : 0);

                case ColumnType.Double: return String.Format(CultureInfo.InvariantCulture, "float({0})", precision >= 1 ? precision : 53);
                case ColumnType.Guid: return "uniqueidentifier";
                case ColumnType.Int16: return "smallint";
                case ColumnType.Int32: return "int";
                case ColumnType.Int64: return "bigint";
                case ColumnType.Object: return "sql_variant";
                //tinyint is unsigned by definition, 0-255, smallint is the best approxmate
                case ColumnType.SByte: return "smallint";
                case ColumnType.Single: return String.Format(CultureInfo.InvariantCulture, "float({0})", precision >= 1 ? precision : 24);
                case ColumnType.String: return String.Format(CultureInfo.InvariantCulture, "nvarchar({0})", length <= 4000 ? length.ToString(CultureInfo.InvariantCulture) : "max");
                case ColumnType.Time: return String.Format(CultureInfo.InvariantCulture, "time({0})", precision >= 0 ? precision : 7);
                // SQL Server doesn't support unsigned, these are the best approximates.
                case ColumnType.UInt16: return "smallint";
                case ColumnType.UInt32: return "int";
                case ColumnType.UInt64: return "bigint";
                case ColumnType.AnsiStringFixedLength: return String.Format(CultureInfo.InvariantCulture, "char({0})", length <= 8000 ? length.ToString(CultureInfo.InvariantCulture) : "8000");
                case ColumnType.StringFixedLength: return String.Format(CultureInfo.InvariantCulture, "nchar({0})", length <= 4000 ? length.ToString(CultureInfo.InvariantCulture) : "4000");
                case ColumnType.Xml: return "xml";
                case ColumnType.DateTime2: return String.Format(CultureInfo.InvariantCulture, "datetime2({0})", precision >= 0 ? precision : 7);
                case ColumnType.DateTimeOffset: return String.Format(CultureInfo.InvariantCulture, "datetimeoffset({0})", precision >= 0 ? precision : 7);

                default: throw new InvalidOperationException("Unknown Data Type");
            }
        }
    }
}