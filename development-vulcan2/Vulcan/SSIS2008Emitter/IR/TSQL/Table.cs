using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.TSQL
{
    public class Table : LogicalObject
    {
        private Columns _columns = new Columns();
        private List<Constraint> _constraintList = new List<Constraint>();
        private List<ForeignKeyConstraint> _foreignKeyConstraintList = new List<ForeignKeyConstraint>();
        private Indexes _indexes = new Indexes();
        private DefaultValues _defaultValues = new DefaultValues();
        private List<CheckAndInsertUniqueColumn> _checkAndInsertUniqueColumns = new List<CheckAndInsertUniqueColumn>();
        private List<InsertOrUpdateUniqueColumn> _insertOrUpdateUniqueColumns = new List<InsertOrUpdateUniqueColumn>();
        private List<LogicalObject> _tags = new List<LogicalObject>();
        private Framework.ConnectionConfiguration _connectionConfiguration;

        public Framework.ConnectionConfiguration ConnectionConfiguration
        {
            get { return _connectionConfiguration; }
            set { _connectionConfiguration = value; }
        }

        public Columns Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public Indexes Indexes
        {
            get { return _indexes; }
            set { _indexes = value; }
        }

        public IList<CheckAndInsertUniqueColumn> CheckAndInsertUniqueColumns
        {
            get { return _checkAndInsertUniqueColumns; }
        }

        public IList<InsertOrUpdateUniqueColumn> InsertOrUpdateUniqueColumns
        {
            get { return _insertOrUpdateUniqueColumns; }
        }

        public DefaultValues DefaultValues
        {
            get { return _defaultValues; }
            set { _defaultValues = value; }
        }

        public IList<Constraint> ConstraintList
        {
            get { return _constraintList; }
        }

        public IList<ForeignKeyConstraint> ForeignKeyConstraintList
        {
            get { return _foreignKeyConstraintList; }
        }

        //Testing Metadata Tags on Table object first!
        public IList<LogicalObject> Tags
        {
            get { return _tags; }
        }

        public void AddForeignKeyConstraint(ForeignKeyConstraint constraint)
        {
            base.AddChild(constraint);
            _foreignKeyConstraintList.Add(constraint);
        }

        public bool IsIdentityColumn(Column column)
        {
            IdentityConstraint c = this.ContainsIdentities();

            if (c != null)
            {
                foreach (Key k in c.Keys)
                {
                    if (k.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IdentityConstraint ContainsIdentities()
        {
            Constraint c = GetKeyConstraint();

            if (c != null && c is IdentityConstraint)
            {
                return (IdentityConstraint)c;
            }
            return null;
        }

        public Constraint GetKeyConstraint()
        {
            foreach (Constraint c in this.ConstraintList)
            {
                if (c is IdentityConstraint || c is PrimaryKeyConstraint)
                {
                    return c;
                }
            }
            return null;
        }
    }

    public class DefaultValues : LogicalObject
    {
        private List<DefaultValue> _defaultValueList = new List<DefaultValue>();

        public IList<DefaultValue> DefaultValueList
        {
            get { return _defaultValueList; }
        }
    }

    public class DefaultValue : LogicalObject
    {
        private string _value;

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    public class CheckAndInsertUniqueColumn : LogicalReference { }

    public class InsertOrUpdateUniqueColumn : LogicalReference { }
}
