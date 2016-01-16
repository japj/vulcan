using System.Text;

namespace AstLowerer.TSqlEmitter
{
    public class TableTSqlEmitter
    {
        private readonly string _name;
        private readonly string _compressionType;
        private readonly ColumnsTSqlEmitter _columnsEmitter;
        private readonly ConstraintTSqlEmitter _constraintEmitter;

        public TableTSqlEmitter(string name, string compressionType)
        {
            _name = name;
            _compressionType = compressionType;
            _columnsEmitter = new ColumnsTSqlEmitter();
            _constraintEmitter = new ConstraintTSqlEmitter();
        }

        public ColumnsTSqlEmitter ColumnsEmitter
        {
            get { return _columnsEmitter; }
        }

        public ConstraintTSqlEmitter ConstraintsEmitter
        {
            get { return _constraintEmitter; }
        }

        public string Emit()
        {
            var tableBuilder = new StringBuilder();
            var tpe = new TemplatePlatformEmitter("CreateTable");
            tpe.Map("Name", _name);
            tpe.Map("CompressionType", _compressionType);
            tpe.Map("Columns", ColumnsEmitter.ColumnsDdl);
            tpe.Map("Constraints", ConstraintsEmitter.CKConstraints);

            tableBuilder.AppendLine(tpe.Emit());
            tableBuilder.AppendLine(ConstraintsEmitter.ForeignConstraints);
            tableBuilder.AppendLine(_constraintEmitter.Indexes);

            return tableBuilder.ToString();
        }
    }
}