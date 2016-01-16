using System;
using System.Globalization;
using System.Text;

namespace AstLowerer.TSqlEmitter
{
    public class ColumnsTSqlEmitter
    {   
        public static void CheckAndAppendSeparator(string separator, StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, separator);
            }
        }

        private readonly StringBuilder _columnsBuilder = new StringBuilder();
        private readonly StringBuilder _columnsList = new StringBuilder();

        public ColumnsTSqlEmitter()
        {
        }

        public string ColumnsDdl
        {
            get { return _columnsBuilder.ToString(); }
        }

        public string ColumnsList
        {
            get { return _columnsList.ToString(); }
        }

        public void AddColumn(string name, string type, bool identity, int identitySeed, int identityIncrement, bool nullable, string defaultValue, bool computed, string computedDefinition)
        {
            CheckAndAppendSeparator(",", _columnsBuilder);

            if (!computed)
            {
                _columnsBuilder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "\t[{0}] {1}{2}{3}{4}\n",
                    name,
                    type,
                    identity ? String.Format(CultureInfo.InvariantCulture, " IDENTITY({0},{1})", identitySeed, identityIncrement) : String.Empty,
                    nullable ? String.Empty : " NOT NULL",
                    String.IsNullOrEmpty(defaultValue) || identity ? String.Empty : " DEFAULT " + defaultValue);

                CheckAndAppendSeparator(",", _columnsList);
                _columnsList.AppendFormat("{0}", name);
            }
            else
            {
                _columnsBuilder.AppendFormat(CultureInfo.InvariantCulture, "\t[{0}] AS {1}\n", name, computedDefinition);
            }
        }
    }
}