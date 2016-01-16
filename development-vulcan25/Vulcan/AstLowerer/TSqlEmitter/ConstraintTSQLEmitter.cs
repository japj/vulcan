using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AST = VulcanEngine.IR.Ast;

namespace AstLowerer.TSqlEmitter
{
    public class ConstraintTSqlEmitter
    {
        private StringBuilder _constraintKeyBuilder = new StringBuilder();
        private StringBuilder _foreignKeyBuilder = new StringBuilder();
        private StringBuilder _indexBuilder = new StringBuilder();

        public string CKConstraints
        {
            get { return _constraintKeyBuilder.ToString(); }
        }

        public string ForeignConstraints
        {
            get { return _foreignKeyBuilder.ToString(); }
        }

        public string Indexes
        {
            get { return _indexBuilder.ToString(); }
        }

        public void AppendForeignKeyConstraintFromReference(AST.Table.AstTableNode table, string refNameOverride, string refName, AST.Table.AstTableNode refTable)
        {
            TemplatePlatformEmitter tpe = new TemplatePlatformEmitter("ForeignKeyConstraintTemplate");
            tpe.Map("TableName", table.SchemaQualifiedName);

            string constraintName;
            if (!String.IsNullOrEmpty(refNameOverride))
            {
                constraintName = String.Format(CultureInfo.InvariantCulture, "[{0}]", refNameOverride);
            }
            else
            {
                constraintName = String.Format(
                    CultureInfo.InvariantCulture,
                    "[FK_{0}_{1}_{2}_{3}]",
                    table.Name,
                    refName,
                    refTable.Name,
                    refTable.PreferredKey.Columns[0].Column.Name);
            }

            if (constraintName.Length >= 128)
            {
                constraintName = String.Format(CultureInfo.InvariantCulture,"[{0}]",constraintName.Substring(0, 127));
            }

            tpe.Map("ConstraintName", constraintName);
            tpe.Map("Column", refName);
            tpe.Map("ForeignKeyTable", refTable.SchemaQualifiedName);
            tpe.Map("ForeignKeyColumn", refTable.PreferredKey.Columns[0].Column.Name);
            _foreignKeyBuilder.Append(tpe.Emit());
            _foreignKeyBuilder.Append("\n\n");
        }

        public void AppendConstraint(AST.Table.AstTableKeyBaseNode keyBase)
        {
            if (keyBase is AST.Table.AstTablePrimaryKeyNode || keyBase is AST.Table.AstTableIdentityNode)
            {
                AppendConstraintBase(keyBase, "PRIMARY KEY", String.Empty);
            }
            else
            {
                AppendConstraintBase(keyBase, String.Empty, keyBase.Unique ? "UNIQUE " : " ");
            }
        }

        private void AppendConstraintBase(AST.Table.AstTableKeyBaseNode constraint, string primaryKeyString, string unique)
        {
            string clustered = constraint.Clustered ? "CLUSTERED" : "NONCLUSTERED";
            string ignoreDupKey = constraint.IgnoreDupKey ? "IGNORE_DUP_KEY = ON" : "IGNORE_DUP_KEY = OFF";
            string padIndex = constraint.PadIndex ? "PAD_INDEX = ON" : "PAD_INDEX = OFF";
            string keys = BuildKeys(constraint);

            var te = new TemplatePlatformEmitter("ConstraintTemplate", String.Format(CultureInfo.InvariantCulture,"[{0}]",constraint.Name), unique + clustered, keys, "WITH(" + padIndex + "," + ignoreDupKey + ")", primaryKeyString);
            _constraintKeyBuilder.Append("," + te.Emit());
            _constraintKeyBuilder.AppendFormat(CultureInfo.InvariantCulture, "\n");
        }

        public void AppendIndex(string tableName, AST.Table.AstTableIndexNode index)
        {
            string unique = index.Unique ? "UNIQUE" : String.Empty;
            string clustered = index.Clustered ? "CLUSTERED" : "NONCLUSTERED";
            string dropExisting = index.DropExisting ? "DROP_EXISTING = ON" : "DROP_EXISTING = OFF";
            string ignoreDupKey = index.IgnoreDupKey ? "IGNORE_DUP_KEY = ON" : "IGNORE_DUP_KEY = OFF";
            string online = index.Online ? "ONLINE = ON" : "ONLINE = OFF";
            string padIndex = index.Online ? "PAD_INDEX = ON" : "PAD_INDEX = OFF";
            string sortInTempdb = index.SortInTempDB ? "SORT_IN_TEMPDB = ON" : "SORT_IN_TEMPDB = OFF";
            string properties = string.Format(CultureInfo.InvariantCulture, "{0},\n{1},\n{2},\n{3},\n{4}", padIndex, sortInTempdb, dropExisting, ignoreDupKey, online);
            string keys = BuildKeys(index.Columns);

            var te = new TemplatePlatformEmitter("CreateIndex", unique, clustered, String.Format(CultureInfo.InvariantCulture,"[{0}]",index.Name), tableName, keys, properties, string.Empty);
            _indexBuilder.Append(te.Emit());
            _indexBuilder.Append("\n");
        }

        // Due to poor abstractions and interfaces on the AST
        private static string BuildKeys(IList<AST.Table.AstTableIndexColumnNode> keyBase)
        {
            var keyBuilder = new StringBuilder();

            foreach (AST.Table.AstTableIndexColumnNode columnNode in keyBase)
            {
                ColumnsTSqlEmitter.CheckAndAppendSeparator(",", keyBuilder);
                keyBuilder.AppendFormat(CultureInfo.InvariantCulture,"[{0}] {1}", columnNode.Column.Name, columnNode.SortOrder);
            }

            return keyBuilder.ToString();
        }

        private static string BuildKeys(AST.Table.AstTableKeyBaseNode keyBase)
        {
            var keyBuilder = new StringBuilder();

            foreach (AST.Table.AstTableKeyColumnNode columnNode in keyBase.Columns)
            {
                ColumnsTSqlEmitter.CheckAndAppendSeparator(",", keyBuilder);
                keyBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}] {1}", columnNode.Column.Name, columnNode.SortOrder);
            }
            return keyBuilder.ToString();
        }
    }
}