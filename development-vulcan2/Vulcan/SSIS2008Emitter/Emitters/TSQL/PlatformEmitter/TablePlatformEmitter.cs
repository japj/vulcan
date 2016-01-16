using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.TSQL;
using Ssis2008Emitter.Emitters.TSQL.SQLBuilder;

namespace Ssis2008Emitter.Emitters.TSQL.PlatformEmitter
{
    public class TablePlatformEmitter : PlatformEmitter
    {
        public string Emit(Table table)
        {
            string sConstraints = new ConstraintsBuilder(table).BuildConstraints();
            TemplatePlatformEmitter te = new TemplatePlatformEmitter("CreateTable",table.Name,new ColumnsBuilder(table, sConstraints.Length > 0).Build(),sConstraints);

            StringBuilder sTable = new StringBuilder(te.Emit(table));
            sTable.Append(new ConstraintsBuilder(table).BuildForeignKeyConstraints());
            sTable.Append(new IndexesBuilder(table.Indexes).Build());
            sTable.Append(new InsertDefaultValuesEmitter().Emit(table));
            return sTable.ToString();
        }
    }

}
