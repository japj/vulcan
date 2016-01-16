using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using VulcanEngine.IR.Ast.Table;

namespace AstLowerer.TSqlEmitter
{
    public class StaticSourceTSqlEmitter
    {
        private readonly AstTableNode _table;

        private readonly StringBuilder _defaultValueBuilder;

        private readonly TemplatePlatformEmitter _defaultValueEmitter;

        private List<string> _columnNames;

        private List<string> _defaultValues;

        public StaticSourceTSqlEmitter(AstTableNode table)
        {
            _table = table;
            _defaultValueBuilder = new StringBuilder();
            _defaultValueEmitter = new TemplatePlatformEmitter("SimpleInsert");

            _columnNames = new List<string>();
            _defaultValues = new List<string>();
        }

        private static string FlattenStringList(IEnumerable<string> items)
        {
            var flattenedList = new StringBuilder();
            bool first = true;
            foreach (string item in items)
            {
                if (!first)
                {
                    flattenedList.Append(',');
                }

                first = false;
                flattenedList.Append(item);
            }

            return flattenedList.ToString();
        }

        public void AddDefaultValue(string columnName, string defaultValue)
        {
            _columnNames.Add(String.Format(CultureInfo.InvariantCulture,"[{0}]",columnName));
            _defaultValues.Add(defaultValue);
        }

        public void CompleteRow()
        {
            // TODO: Add validation logic here
            _defaultValueEmitter.Map("Table", _table.SchemaQualifiedName);
            _defaultValueEmitter.Map("Columns", FlattenStringList(_columnNames));
            _defaultValueEmitter.Map("Values", FlattenStringList(_defaultValues));

            _defaultValueBuilder.AppendLine(_defaultValueEmitter.Emit());
            _columnNames.Clear();
            _defaultValues.Clear();
        }

        public string Emit()
        {
            var tableBuilder = new StringBuilder();
            
            // TODO: emit this only when there is an identity key
            if (_table.HasIdentityKey)
            {
                tableBuilder.AppendFormat("\nSET IDENTITY_INSERT {0} ON\n", _table.SchemaQualifiedName);
                tableBuilder.AppendLine(_defaultValueBuilder.ToString());
                tableBuilder.AppendFormat("\nSET IDENTITY_INSERT {0} OFF\n", _table.SchemaQualifiedName);
            }
            else
            {
                tableBuilder.Append(_defaultValueBuilder.ToString());
            }

            return tableBuilder.ToString();
        }
    }
}