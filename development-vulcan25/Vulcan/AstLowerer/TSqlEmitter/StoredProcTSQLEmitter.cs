using System;
using System.Globalization;
using System.Text;
using AST = VulcanEngine.IR.Ast;

namespace AstLowerer.TSqlEmitter
{
    public class StoredProcTSqlEmitter
    {
        private readonly StringBuilder _columnsBuilder = new StringBuilder();
        private readonly string _name;
        private readonly string _body;

        public StoredProcTSqlEmitter(AST.Task.AstStoredProcNode astNode)
        {
            _name = astNode.SchemaQualifiedName;
            _body = astNode.Body;

            foreach (AST.Task.AstStoredProcColumnNode storedProcedureColumn in astNode.Columns)
            {
                string physicalType =
                    TSqlTypeTranslator.Translate(
                        storedProcedureColumn.ColumnType,
                        storedProcedureColumn.Length,
                        storedProcedureColumn.Precision,
                        storedProcedureColumn.Scale,
                        storedProcedureColumn.CustomType);

                AddColumn(storedProcedureColumn.Name, physicalType, storedProcedureColumn.Default, storedProcedureColumn.IsOutput, storedProcedureColumn.IsReadOnly);
            }
        }

        public void AddColumn(string name, string type, string defaultValue, bool isOutput, bool isReadOnly)
        {
            if (_columnsBuilder.Length > 0)
            {
                _columnsBuilder.Append(",");
            }

            string outputValue = isOutput ? "OUTPUT" : String.Empty;
            string readonlyValue = isReadOnly ? "READONLY" : String.Empty;

            if (defaultValue == null)
            {
                _columnsBuilder.AppendFormat(CultureInfo.InvariantCulture, "@{0} {1} {2} {3}\n", name, type, outputValue, readonlyValue);
            }
            else
            {
                _columnsBuilder.AppendFormat(CultureInfo.InvariantCulture, "@{0} {1} = {2} {3} {4}\n", name, type, defaultValue, outputValue, readonlyValue); 
            }
        }

        public string Emit()
        {
            var tpe = new TemplatePlatformEmitter("StoredProc");
            tpe.Map("Name", _name);
            tpe.Map("Columns", _columnsBuilder.ToString());
            tpe.Map("Body", _body);
            return tpe.Emit();
        }
    }
}