using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.TSQL;
using Ssis2008Emitter.Emitters.TSQL.SQLBuilder;

namespace Ssis2008Emitter.Emitters.TSQL.PlatformEmitter
{
    internal class InsertDefaultValuesEmitter : PlatformEmitter
    {
        public string Emit(Table table)
        {
            if (table.DefaultValues == null)
            {
                return string.Empty;
            }

            bool bContainsIdentities;
            string columns = new ColumnsBuilder(table).BuildDefaultValues(out bContainsIdentities);
            StringBuilder str = new StringBuilder();

            if (bContainsIdentities)
            {
                str.AppendFormat("\nSET IDENTITY_INSERT {0} ON\n", table.Name);
            }
            str.Append("\n");
            foreach (DefaultValue val in table.DefaultValues.DefaultValueList)
            {
                str.Append(new TemplatePlatformEmitter("InsertDefaultValues", table.Name, columns, val.Value).Emit(val));
                str.Append("\n");
            }

            if (bContainsIdentities)
            {
                str.AppendFormat("\nSET IDENTITY_INSERT {0} OFF", table.Name);
            }

            str.Append("\n");
            return str.ToString();
        }
    }
}
