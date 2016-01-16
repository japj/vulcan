using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.TSQL;

namespace Ssis2008Emitter.Emitters.TSQL.SQLBuilder
{
    internal class ColumnsBuilder : SQLBuilder
    {
        private Table _table;

        public ColumnsBuilder(Table table) : this(table, false) { }

        public ColumnsBuilder(Table table, bool bAppendSeparator) : base(bAppendSeparator)
        {
            _table = table;
        }

        public string Build()
        {
            Clear();

            foreach (Column col in _table.Columns.ColumnList)
            {
                CheckAndAppendSeparator();
                Append(col);
            }
            return ToString();
        }

        public string BuildDefaultValues(out bool bContainsIdentities)
        {
            Clear(SEPARATOR_COMMA);
            bContainsIdentities = false;

            foreach (Column column in _table.Columns.ColumnList)
            {
                if (!String.IsNullOrEmpty(column.Computed) || column.IsComputed)
                {
                    continue;
                }
                CheckAndAppendSeparator();
                AppendDefaultValueColumn(column);

                if (_table.IsIdentityColumn(column))
                {
                    bContainsIdentities = true;
                }
            }
            return ToString();
        }

        private void Append(Column column)
        {
            if(String.IsNullOrEmpty(column.Computed) && !column.IsComputed)
            {
            _stringBuilder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
            "\t[{0}] {1}{2}{3}{4}",
            column.Name,
            column.Type,
            _table.IsIdentityColumn(column) ? " IDENTITY(1,1)" : "",
            column.IsNullable ? "" : " NOT NULL",
            String.IsNullOrEmpty(column.Default) || _table.IsIdentityColumn(column) ? "" : " DEFAULT " + column.Default
            );
            }
            else
            {
                _stringBuilder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "\t[{0}] {1}",
                    column.Name,
                    column.Computed
                    );
            }
        }

        private void AppendDefaultValueColumn(Column column)
        {
            _stringBuilder.AppendFormat("[{0}]", column.Name);
        }
    }
}
