using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.IR.Ast.Table;

namespace Ssis2008Emitter.Emitters.TSQL
{
    public class PhysicalTypeTranslator
    {
        public static string Translate(ColumnType type, int length, int precision, int scale, string customType)
        {
            string physicalTypeString = String.Empty;
            length = length >= 1 ? length : 1;

            switch(type)
            {
                case ColumnType.CUSTOM:
                    physicalTypeString = customType;
                    break;
                case ColumnType.INT32:
                    physicalTypeString = "INTEGER";
                    break;
                case ColumnType.UINT32:
                    physicalTypeString = "INTEGER";
                    break;
                case ColumnType.INT64:
                    physicalTypeString = "BIGINT";
                    break;
                case ColumnType.UINT64:
                    physicalTypeString = "BIGINT";
                    break;
                case ColumnType.DOUBLE:
                    physicalTypeString = "FLOAT";
                    break;
                case ColumnType.FLOAT:
                    physicalTypeString = "FLOAT";
                    break;
                case ColumnType.BOOL:
                    physicalTypeString = "BIT";
                    break;
                case ColumnType.DATE:
                    physicalTypeString = "DATE";
                    break;
                case ColumnType.TIME:
                    physicalTypeString = String.Format("TIME({0})",scale >= 0 ? scale : 7);
                    break;
                case ColumnType.DATETIME:
                    //physicalTypeString = String.Format("DATETIMEOFFSET({0})", scale >= 0 ? scale : 7);
                    physicalTypeString = "DATETIME2(7)";
                    break;
                case ColumnType.DECIMAL:
                    physicalTypeString = String.Format("DECIMAL({0},{1})", precision >= 1 ? precision : 38, scale >= 0 ? scale : 0);
                    break;
                case ColumnType.BINARY:
                    physicalTypeString = String.Format("BINARY VARYING({0})", length <= 8000 ? length.ToString() : "max");
                    break;
                case ColumnType.WSTR:
                    physicalTypeString = String.Format("NATIONAL CHARACTER VARYING({0})", length <= 4000 ? length.ToString() : "max");
                    break;
                case ColumnType.STR:
                    physicalTypeString = String.Format("CHARACTER VARYING({0})", length <= 8000 ? length.ToString() : "max");
                    break;
                default:
                    throw new Exception("Unknown Data Type");
            }
            return physicalTypeString;
        }
    }
}
