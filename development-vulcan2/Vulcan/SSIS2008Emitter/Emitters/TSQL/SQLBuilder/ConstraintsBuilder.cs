using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.TSQL;
using Ssis2008Emitter.Emitters.TSQL.PlatformEmitter;

namespace Ssis2008Emitter.Emitters.TSQL.SQLBuilder
{
    internal class ConstraintsBuilder : SQLBuilder
    {
        private Table _table;

        public ConstraintsBuilder(Table table) : this(table, false) { }

        public ConstraintsBuilder(Table table, bool bAppendSeparator) : base(bAppendSeparator)
        {
            _table = table;
        }

        public string BuildConstraints()
        {
            Clear();
            if (_table != null && _table.ConstraintList != null)
            {
                foreach (Constraint k in _table.ConstraintList)
                {
                    CheckAndAppendSeparator();
                    Append(k);
                }
            }
            return ToString();
        }

        public string BuildForeignKeyConstraints()
        {
            Clear();
            if (_table != null && _table.ForeignKeyConstraintList != null)
            {
                foreach (ForeignKeyConstraint fkey in _table.ForeignKeyConstraintList)
                {
                    AppendNewLine();
                    Append(fkey);
                }
            }
            return ToString();
        }

        private void Append(Constraint constraint)
        {
            if (constraint is PrimaryKeyConstraint || constraint is IdentityConstraint)
            {
                AppendConstraintBase(constraint, "PRIMARY KEY", "");
            }
            else
            {
                AppendConstraintBase(constraint, "", "UNIQUE ");
            }
        }

        private void Append(ForeignKeyConstraint constraint)
        {
            string strConstraintName;

            if (String.IsNullOrEmpty(constraint.Name) || !constraint.Name.StartsWith("FK_"))
            {
                strConstraintName = String.Format(System.Globalization.CultureInfo.InvariantCulture, "FK_{0}_{1}", constraint.Parent.Name, constraint.Name);
            }
            else
            {
                strConstraintName = constraint.Name;
            }

            if (strConstraintName.Length >= 128)
            {
                strConstraintName = strConstraintName.Substring(0, 127);
            }

            TemplatePlatformEmitter te = new TemplatePlatformEmitter(
                "ForeignKeyConstraintTemplate",
                constraint.Parent.Name,
                strConstraintName,
                new KeysBuilder(constraint.LocalColumnList).BuildForeignKeys(),
                constraint.Table,
                new KeysBuilder(constraint.ForeignColumnList).BuildForeignKeys()
            );
            _stringBuilder.Append(te.Emit(constraint));
            _stringBuilder.Append(NEWLINE);
        }

        private void AppendConstraintBase(IndexBase constraint, string primaryKeyString, string unique)
        {
            string clustered = constraint.Clustered ? "CLUSTERED" : "NONCLUSTERED";
            string ignoreDupKey = constraint.IgnoreDupKey ? "IGNORE_DUP_KEY = ON" : "IGNORE_DUP_KEY = OFF";
            string padIndex = constraint.PadIndex ? "PAD_INDEX = ON" : "PAD_INDEX = OFF";
            string kName;
            string keys = new KeysBuilder(constraint.Keys).Build(constraint.Name, out kName);

            TemplatePlatformEmitter te = new TemplatePlatformEmitter("ConstraintTemplate", kName, unique + clustered, keys, "WITH(" + padIndex + "," + ignoreDupKey + ")", primaryKeyString);
            _stringBuilder.Append(te.Emit(constraint));
        }
    }
}
