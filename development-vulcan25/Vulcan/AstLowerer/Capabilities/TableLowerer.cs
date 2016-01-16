using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using AstLowerer.TSqlEmitter;
using Ssis2008Emitter.IR.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Dimension;
using VulcanEngine.IR.Ast.Fact;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;

namespace AstLowerer.Capabilities
{
    public static class TableLowerer
    {
        public static AstPackageNode ProcessTable(AstTableNode tableNode)
        {
            string packageTypeName = "Table";

            if (tableNode is AstDimensionNode)
            {
                packageTypeName = "Dimension";
            }
            else if (tableNode is AstFactNode)
            {
                packageTypeName = "FactTable";
            }

            // TODO: Fix this null parent node
            var packageNode = new AstPackageNode(tableNode.ParentItem);
            packageNode.Name = tableNode.Name;
            packageNode.PackageType = packageTypeName;
            packageNode.Emit = tableNode.Emit;

            LowerTable(packageNode, tableNode, tableNode.SchemaQualifiedName, false);

            return packageNode;
        }

        internal static void LowerTable(AstContainerTaskNode containerNode, AstTableNode tableNode, string executeSqlTaskName, bool executeDuringDesignTime)
        {
            var tableEmitter = new TableTSqlEmitter(tableNode.SchemaQualifiedName, tableNode.CompressionType.ToString().ToUpper(CultureInfo.InvariantCulture));

            foreach (AstTableColumnBaseNode columnBase in tableNode.Columns)
            {
                ProcessAstTableColumnBaseNode(tableNode, tableEmitter.ColumnsEmitter, columnBase);

                var tableReference = columnBase as AstTableColumnTableReferenceNode;
                var dimReference = columnBase as AstTableColumnDimensionReferenceNode;

                if (tableReference != null && tableReference.EnforceForeignKeyConstraint)
                {
                    tableEmitter.ConstraintsEmitter.AppendForeignKeyConstraintFromReference(tableNode, tableReference.ForeignKeyNameOverride, tableReference.Name, tableReference.Table);
                }

                if (dimReference != null && dimReference.EnforceForeignKeyConstraint)
                {
                    tableEmitter.ConstraintsEmitter.AppendForeignKeyConstraintFromReference(tableNode, dimReference.ForeignKeyNameOverride, dimReference.Name, dimReference.Dimension);
                }
            }

            foreach (AstTableKeyBaseNode keyBase in tableNode.Keys)
            {
                tableEmitter.ConstraintsEmitter.AppendConstraint(keyBase);
            }

            foreach (AstTableIndexNode index in tableNode.Indexes)
            {
                tableEmitter.ConstraintsEmitter.AppendIndex(tableNode.SchemaQualifiedName, index);
            }

            // TODO: Fix this null parent node
            var createTableExecuteSqlTaskNode = new AstExecuteSqlTaskNode(containerNode)
                                                    {
                                                        Name = StringManipulation.NameCleanerAndUniqifier(executeSqlTaskName),
                                                        ResultSet = ExecuteSqlResultSet.None,
                                                        Connection = tableNode.Connection,
                                                        ExecuteDuringDesignTime = executeDuringDesignTime
                                                    };

            createTableExecuteSqlTaskNode.Query = new AstExecuteSqlQueryNode(createTableExecuteSqlTaskNode) { QueryType = QueryType.Standard, Body = tableEmitter.Emit() };
            containerNode.Tasks.Add(createTableExecuteSqlTaskNode);

            bool hasPermissions = false;
            var permissionBuilder = new StringBuilder();
            foreach (var permission in tableNode.Permissions)
            {
                hasPermissions = true;
                permissionBuilder.AppendLine(PermissionsLowerer.ProcessPermission(tableNode, permission));
            }

            foreach (var column in tableNode.Columns)
            {
                foreach (var permission in column.Permissions)
                {
                    hasPermissions = true;
                    permissionBuilder.AppendLine(PermissionsLowerer.ProcessPermission(column, permission));
                }
            }

            if (hasPermissions)
            {
                var permissionsExecuteSqlTask = new AstExecuteSqlTaskNode(containerNode)
                {
                    Name = "__SetPermissions",
                    Connection = tableNode.Connection,
                };
                permissionsExecuteSqlTask.Query = new AstExecuteSqlQueryNode(permissionsExecuteSqlTask)
                {
                    Body = permissionBuilder.ToString(),
                    QueryType = QueryType.Standard
                };
                containerNode.Tasks.Add(permissionsExecuteSqlTask);
            }

            if (tableNode.CustomExtensions != null)
            {
                containerNode.Tasks.Add(tableNode.CustomExtensions);
            }

            foreach (var source in tableNode.Sources)
            {
                var staticSource = source as AstTableStaticSourceNode;
                if (staticSource != null && staticSource.Rows.Count > 0)
                {
                    if (staticSource.EmitMergePackage)
                    {
                        // TODO: This is nasty - we need a way to reference packages and emit paths at lowering time
                        var executeMergePackage = new AstExecutePackageTaskNode(containerNode);
                        executeMergePackage.Name = "__ExecuteMergePackage";
                        executeMergePackage.Package = staticSource.LoweredPackage;
                        containerNode.Tasks.Add(executeMergePackage);
                    }
                    else
                    {
                        containerNode.Tasks.Add(StaticSourcesLowerer.CreateInsertExecuteSql(staticSource, containerNode, tableNode));
                    }
                }
            }
        }

        // This needs its own emitter
        private static void ProcessAstTableColumnBaseNode(AstTableNode tableNode, ColumnsTSqlEmitter columnsEmitter, AstTableColumnBaseNode columnBase)
        {
            var tableReference = columnBase as AstTableColumnTableReferenceNode;
            var dimReference = columnBase as AstTableColumnDimensionReferenceNode;
            var hashKey = columnBase as AstTableHashedKeyColumnNode;

            if (hashKey != null)
            {
                var hashBytesBuilder = new StringBuilder();
                foreach (AstTableKeyColumnNode keyColumn in hashKey.Constraint.Columns)
                {
                    string expression = "+ HASHBYTES('SHA1',{0})";
                    switch (keyColumn.Column.ColumnType)
                    {
                        case ColumnType.AnsiString:
                        case ColumnType.AnsiStringFixedLength:
                        case ColumnType.String:
                        case ColumnType.StringFixedLength:
                            expression = String.Format(CultureInfo.InvariantCulture, expression, String.Format(CultureInfo.InvariantCulture, "UPPER(RTRIM(LTRIM([{0}])))", keyColumn.Column.Name));
                            break;

                        case ColumnType.Int16:
                        case ColumnType.Int32:
                        case ColumnType.Int64:
                        case ColumnType.UInt16:
                        case ColumnType.UInt32:
                        case ColumnType.UInt64:
                            expression = String.Format(CultureInfo.InvariantCulture, expression, String.Format(CultureInfo.InvariantCulture, "CONVERT(binary varying(64),{0})", keyColumn.Column.Name));
                            break;
                        default:
                            expression = String.Format(CultureInfo.InvariantCulture, expression, keyColumn.Column.Name);
                            break;
                    }

                    hashBytesBuilder.Append(expression);
                }

                string hashExpression = String.Format(CultureInfo.InvariantCulture, "(CONVERT(varbinary(32),HASHBYTES('SHA1',{0})))", hashBytesBuilder.ToString().Substring(1));
                hashExpression = String.Format(CultureInfo.InvariantCulture, "{0} PERSISTED NOT NULL UNIQUE", hashExpression);
                columnsEmitter.AddColumn(hashKey.Name, null, false, 0, 0, true, string.Empty, true, hashExpression);
            }
            else if (tableReference != null)
            {
                BindTableReference(tableNode, tableReference.Name, tableReference.Table, tableReference.IsNullable, columnsEmitter);
            }
            else if (dimReference != null)
            {
                BindTableReference(tableNode, dimReference.Name, dimReference.Dimension, dimReference.IsNullable, columnsEmitter);
            }
            else if (columnBase != null)
            {
                string type = TSqlTypeTranslator.Translate(columnBase.ColumnType, columnBase.Length, columnBase.Precision, columnBase.Scale, columnBase.CustomType);
                bool identity = false;
                int seed = 1;
                int increment = 1;
                foreach (AstTableKeyBaseNode keyBase in tableNode.Keys)
                {
                    var identityNode = keyBase as AstTableIdentityNode;
                    if (identityNode != null)
                    {
                        foreach (AstTableKeyColumnNode keyColNode in identityNode.Columns)
                        {
                            if (keyColNode.Column.Name.Equals(columnBase.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                identity = true;
                                seed = identityNode.Seed;
                                increment = identityNode.Increment;
                            }
                        }
                    }
                }

                columnsEmitter.AddColumn(columnBase.Name, type, identity, seed, increment, columnBase.IsNullable, columnBase.Default, columnBase.IsComputed, columnBase.Computed);
            }
        }

        private static void BindTableReference(AstTableNode tableNode, string refName, AstTableNode refTable, bool nullable, ColumnsTSqlEmitter columnsEmitter)
        {
            // TODO: This is wrong in general.  We need to ensure that this is always a single column constraint
            AstTableKeyBaseNode primaryKey = refTable.PreferredKey;
            if (primaryKey == null)
            {
                MessageEngine.Trace(tableNode, Severity.Warning, "V0243", "Table {0} lacks a primary, identity, or unique key", tableNode.Name);
                return;
            }

            // TODO: We currently support only a single primary key column.  This should be fixed :)
            foreach (AstTableKeyColumnNode keyCol in primaryKey.Columns)
            {
                string type = TSqlTypeTranslator.Translate(
                    keyCol.Column.ColumnType,
                    keyCol.Column.Length,
                    keyCol.Column.Precision,
                    keyCol.Column.Scale,
                    keyCol.Column.CustomType);
                columnsEmitter.AddColumn(refName, type, false, 0, 0, nullable, keyCol.Column.Default, false, String.Empty);
            }
        }

        private static string FlattenStringList(IEnumerable<string> items)
        {
            return FlattenStringList(items, ",");
        }

        private static string FlattenStringList(IEnumerable<string> items, string separator)
        {
            var flattenedList = new StringBuilder();
            bool first = true;
            foreach (string item in items)
            {
                if (!first)
                {
                    flattenedList.Append(separator);
                }

                first = false;
                flattenedList.Append(item);
            }

            return flattenedList.ToString();
        }

        private static List<string> EmitColumnList(AstTableNode astTableNode, bool emitOnlyAssignable)
        {
            var columnNames = new List<string>();
            foreach (AstTableColumnBaseNode column in astTableNode.Columns)
            {
                if (!emitOnlyAssignable || column.IsAssignable)
                {
                    columnNames.Add(column.Name);
                }
            }

            return columnNames;
        }

        private static List<string> EmitDefaultValueList(AstTableNode astTableNode)
        {
            var columnValues = new List<string>();

            foreach (AstTableColumnBaseNode column in astTableNode.Columns)
            {
                if (column.IsAssignable)
                {
                    if (!String.IsNullOrEmpty(column.Default))
                    {
                        columnValues.Add(column.Default);
                    }
                    else if (column.IsNullable)
                    {
                        columnValues.Add("NULL");
                    }
                    else
                    {
                        columnValues.Add(column.DefaultValue);
                    }
                }
            }

            return columnValues;
        }

        public static string EmitSelectAllStatement(AstTableNode astTableNode, IEnumerable<string> columnNames)
        {
            var selectTemplate = new TemplatePlatformEmitter("SimpleSelect");
            selectTemplate.Map("Table", astTableNode.SchemaQualifiedName);
            selectTemplate.Map("Columns", FlattenStringList(columnNames));
            return selectTemplate.Emit();
        }

        public static string EmitSelectAllStatement(AstTableNode astTableNode)
        {
            var columnNames = EmitColumnList(astTableNode, false);
            return EmitSelectAllStatement(astTableNode, columnNames);
        }

        public static string EmitUpdateStatement(AstTableNode astTableNode, IEnumerable<TableColumnValueMapping> assignments, IEnumerable<TableColumnValueMapping> conditions)
        {
            var updateTemplate = new TemplatePlatformEmitter("SimpleUpdate");
            updateTemplate.Map("Table", astTableNode.SchemaQualifiedName);
            var assignmentStrings = new List<string>();
            foreach (var assignmentPair in assignments)
            {
                assignmentStrings.Add(String.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", assignmentPair.ColumnName, assignmentPair.OperatorString, assignmentPair.ColumnValue));
            }

            updateTemplate.Map("ColumnValuePairs", FlattenStringList(assignmentStrings));

            var whereTemplate = new TemplatePlatformEmitter("SimpleWhere");
            var whereStrings = new List<string>();
            foreach (var wherePair in conditions)
            {
                whereStrings.Add(String.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", wherePair.ColumnName, wherePair.OperatorString, wherePair.ColumnValue));
            }

            whereTemplate.Map("ColumnValuePairs", FlattenStringList(whereStrings, " AND "));

            return String.Format(CultureInfo.InvariantCulture, "{0} {1}", updateTemplate.Emit(), whereTemplate.Emit());
        }

        public static string EmitInsertDefaultRowStatement(AstTableNode astTableNode)
        {
            string columnNames = FlattenStringList(EmitColumnList(astTableNode, true));
            string columnDefaultValues = FlattenStringList(EmitDefaultValueList(astTableNode));

            if (columnDefaultValues.Length > 0)
            {
                var insertTemplate = new TemplatePlatformEmitter("SimpleInsert", astTableNode.SchemaQualifiedName, columnNames, columnDefaultValues);
                return insertTemplate.Emit();
            }

            MessageEngine.Trace(astTableNode, Severity.Error, "V0142", "No assignable columns detected on table {0}", astTableNode.Name);

            throw new InvalidOperationException("No Assignable Columns Detected.");
        }
    }
}
