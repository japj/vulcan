using System;
using System.Collections.Generic;
using System.Text;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.IR.TSQL;

namespace Ssis2008Emitter.Emitters.TSQL.SQLBuilder
{
    internal class SPColumn
    {
        public Column Column;
        public bool IsIdentity;
        public bool IsKey;
    }

    internal abstract class SPColumnsBuilder : SQLBuilder
    {
        protected List<SPColumn> _columns = new List<SPColumn>();
        protected SPColumn _identityColumn;
        protected SPColumn _keyColumn;
        protected Table _table;

        public SPColumnsBuilder(Columns columns, bool bAppendSeparator) : base(bAppendSeparator)
        {
            foreach (Column column in columns.ColumnList)
            {
                _columns.Add(IsKeyOrIdentityColumn(column));
            }

            if (columns.Parent != null && columns.Parent is Table)
            {
                _table = (Table)columns.Parent;
            }
            else if (columns.Parent == null)
            {
                MessageEngine.Global.Trace(Severity.Error, Resources.ParentIsNull, columns.ToString());
            }
            else
            {
                MessageEngine.Global.Trace(Severity.Error, Resources.ColumnsParentIsNotTable, columns.Parent.Name);
            }
            _identityColumn = null;
            _keyColumn = null;
        }

        public Column KeyColumn
        {
            get { return _keyColumn == null ? null : _keyColumn.Column; }
        }

        public Column IdentityColumn
        {
            get { return _identityColumn == null ? null : _identityColumn.Column; }
        }

        public Column KeyOrIdentityColumn
        {
            get { return KeyColumn != null ? KeyColumn : IdentityColumn; }
        }

        public string KeyOrIdentiyColumnType
        {
            get
            {
                if (_keyColumn != null)
                {
                    return "PrimaryKey";
                }
                else if (_identityColumn != null)
                {
                    return "Identity";
                }
                return string.Empty;
            }
        }

        public string BuildParameters()
        {
            Clear();
            foreach (SPColumn col in _columns)
            {
                CheckAndAppendSeparator();
                AppendParameter(col);
            }
            return ToString();
        }

        public string BuildExecArguments()
        {
            Clear();
            foreach (SPColumn col in _columns)
            {
                CheckAndAppendSeparator();
                AppendExecArgument(col);
            }
            return ToString();
        }

        protected void AppendParameter(SPColumn column)
        {
            _stringBuilder.AppendFormat("\t@{0} {1} {2}", column.Column.Name, column.Column.Type, column.IsKey ? "OUTPUT" : "");
        }

        protected void AppendExecArgument(SPColumn column)
        {
            _stringBuilder.AppendFormat("\t\t@{0}{1}", column.Column.Name, column.IsKey ? " OUTPUT" : "");
        }

        protected void AppendUpdateParameter(SPColumn column)
        {
            if (!column.IsKey)
            {
                _stringBuilder.AppendFormat("{0} = @{0}", column.Column.Name);
            }
        }

        private SPColumn IsKeyOrIdentityColumn(Column column)
        {
            SPColumn sqlColumn = new SPColumn();
            sqlColumn.Column = column;
            sqlColumn.IsKey = false;
            sqlColumn.IsIdentity = false;
            /* TODO
            foreach (LogicalReference r in column.References)
            {
                if (r is Key)
                {
                    if (r.Parent is PrimaryKeyConstraint)
                    {
                        sqlColumn.IsKey = true;
                        _keyColumn = sqlColumn;
                        break;
                    }

                    if (r.Parent is IdentityConstraint)
                    {
                        sqlColumn.IsIdentity = true;
                        _identityColumn = sqlColumn;
                        break;
                    }
                }
            }*/
            return sqlColumn;
        }
    }
}
