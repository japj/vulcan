using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.TSQL;

namespace Ssis2008Emitter.Emitters.TSQL.SQLBuilder
{
    internal class StoredProcColumnsBuilder : SQLBuilder
    {
        private StoredProcColumns _columns;

        public StoredProcColumnsBuilder(StoredProcColumns columns) : this(columns, false) { }

        public StoredProcColumnsBuilder(StoredProcColumns columns, bool bAppendSeparator) : base(bAppendSeparator)
        {
            _columns = columns;
        }

        public string Build()
        {
            Clear();
            foreach (StoredProcColumn column in _columns.ColumnList)
            {
                CheckAndAppendSeparator();
                Append(column);
            }
            return ToString();
        }

        private void Append(StoredProcColumn column)
        {
            string output = column.IsOutput ? "OUTPUT" : "";
            string defaultValue = string.IsNullOrEmpty(column.Default) ? string.Empty : " = " + column.Default;
            _stringBuilder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "@{0} {1} {2} {3}", column.Name, column.Type, defaultValue, output);
        }
    }
}
