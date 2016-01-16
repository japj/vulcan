using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.TSQL;
using Ssis2008Emitter.Emitters.TSQL.PlatformEmitter;

namespace Ssis2008Emitter.Emitters.TSQL.SQLBuilder
{
    internal class IndexesBuilder : SQLBuilder
    {
        private Indexes _indexes;

        public IndexesBuilder(Indexes indexes) : this(indexes, false) { }

        public IndexesBuilder(Indexes indexes, bool bAppendSeparator) : base(bAppendSeparator)
        {
            _indexes = indexes;
        }

        public string Build()
        {
            Clear();
            if (_indexes != null && _indexes.Parent is Table)
            {
                foreach (Index index in _indexes.IndexList)
                {
                    AppendNewLine();
                    Append(_indexes.Parent.Name, index);
                }
            }
            return ToString();
        }

        private void Append(string tableName, Index index)
        {
            string unique = index.Unique ? "UNIQUE" : "";
            string clustered = index.Clustered ? "CLUSTERED" : "NONCLUSTERED";
            string dropExisting = index.DropExisting ? "DROP_EXISTING = ON" : "DROP_EXISTING = OFF";
            string ignoreDupKey = index.IgnoreDupKey ? "IGNORE_DUP_KEY = ON" : "IGNORE_DUP_KEY = OFF";
            string online = index.Online ? "ONLINE = ON" : "ONLINE = OFF";
            string padIndex = index.Online ? "PAD_INDEX = ON" : "PAD_INDEX = OFF";
            string sortInTempdb = index.SortInTempdb ? "SORT_IN_TEMPDB = ON" : "SORT_IN_TEMPDB = OFF";
            string sProperties = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},\n{1},\n{2},\n{3},\n{4}", padIndex, sortInTempdb, dropExisting, ignoreDupKey, online);
            string kName;
            string keys = new KeysBuilder(index.Keys).Build(index.Name, out kName);

            TemplatePlatformEmitter te = new TemplatePlatformEmitter("CreateIndex", unique, clustered, kName, tableName, keys, sProperties, string.Empty);
            _stringBuilder.Append(te.Emit(index));
            _stringBuilder.Append(NEWLINE);
        }
    }
}
