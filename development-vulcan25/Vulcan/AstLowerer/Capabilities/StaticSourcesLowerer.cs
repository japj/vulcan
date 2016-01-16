using System;
using System.Collections.Generic;
using System.Globalization;
using AstFramework;
using AstFramework.Model;
using AstLowerer.TSqlEmitter;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Dimension;
using VulcanEngine.IR.Ast.Fact;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;

namespace AstLowerer.Capabilities
{
    public static class StaticSourcesLowerer
    {
        public static string GetPackageType(AstTableNode tableNode)
        {
            if (tableNode is AstDimensionNode)
            {
                return "Dimension";
            }

            if (tableNode is AstFactNode)
            {
                return "FactTable";
            }

            return "Table";
        }

        private static string GetInsertStatements(AstTableNode table, AstTableStaticSourceNode staticSource)
        {
            var staticSourceEmitter = new StaticSourceTSqlEmitter(table);
            foreach (AstStaticSourceRowNode row in staticSource.Rows)
            {
                foreach (AstStaticSourceColumnValueNode columnValue in row.ColumnValues)
                {
                    staticSourceEmitter.AddDefaultValue(columnValue.Column.Name, columnValue.Value);
                }

                staticSourceEmitter.CompleteRow();
            }

            return staticSourceEmitter.Emit();
        }

        public static IEnumerable<AstPackageNode> ProcessTableStaticSource(AstTableNode astTableNode)
        {
            var packageList = new List<AstPackageNode>();
            foreach (var source in astTableNode.Sources)
            {
                var staticSource = source as AstTableStaticSourceNode;
                if (staticSource != null && staticSource.Rows.Count > 0)
                {
                    var table = staticSource.ParentItem as AstTableNode;
                    if (table != null && staticSource.EmitMergePackage)
                    {
                        if (table.PreferredKey == null)
                        {
                            MessageEngine.Trace(table, Severity.Error, "L0110", "Table {0} does not contain a primary key and therefore cannot use Static Source Merging.  Add a primary key or set EmitMergePackage to false.", table.Name);
                        }

                        var package = new AstPackageNode(table.ParentItem)
                        {
                            Emit = table.Emit,
                            Log = false,
                            Name = staticSource.Name,
                            PackageFolderSubpath = table.Name,
                            PackageType = GetPackageType(table)
                        };

                        // Staging Container
                        var staging = new AstStagingContainerTaskNode(package)
                        {
                            Log = false,
                            Name = Utility.NameCleanerAndUniqifier(source.Name + "_Stg"),
                        };

                        var stagingCloneTable = new AstTableCloneNode(staging)
                        {
                            Name =
                            String.Format(CultureInfo.InvariantCulture, "__{0}_Static", table.Name),
                            Connection = table.Connection,
                            Table = table
                        };
                        staging.Tables.Add(stagingCloneTable);
                        CloneTableLowerer.LowerCloneTable(stagingCloneTable);

                        staging.Tasks.Add(CreateInsertExecuteSql(staticSource, staging, stagingCloneTable));
                        staging.Tasks.Add(CreateMergeTask(table, staging, stagingCloneTable));

                        package.Tasks.Add(staging);
                        packageList.Add(package);
                        staticSource.LoweredPackage = package;
                    }
                }
            }

            return packageList;
        }

        public static AstExecuteSqlTaskNode CreateInsertExecuteSql(AstTableStaticSourceNode staticSource, IFrameworkItem insertParent, AstTableNode insertTargetTable)
        {
            var executeSql = new AstExecuteSqlTaskNode(insertParent)
                                 {
                                     Connection = insertTargetTable.Connection,
                                     ExecuteDuringDesignTime = false,
                                     Name = Utility.NameCleanerAndUniqifier(insertTargetTable + "_Insert"),
                                 };

            executeSql.Query = new AstExecuteSqlQueryNode(executeSql)
                                   {
                                       Body = GetInsertStatements(insertTargetTable, staticSource),
                                       QueryType = QueryType.Standard
                                   };
            return executeSql;
        }

        private static AstMergeTaskNode CreateMergeTask(AstTableNode table, AstStagingContainerTaskNode staging, AstTableCloneNode stagingCloneTable)
        {
            var merge = new AstMergeTaskNode(staging)
                            {
                                Name = Utility.NameCleanerAndUniqifier(table + "_SSMerge"),
                                SourceTable = stagingCloneTable,
                                TargetConstraint = stagingCloneTable.Table.PreferredKey,
                                UnspecifiedColumnDefaultUsageType = MergeColumnUsage.CompareUpdateInsert
                            };

            // Detect identity keys, if so add an insert- merge-column-usage for said key
            foreach (var columnRef in stagingCloneTable.Table.PreferredKey.Columns)
            {
                if (columnRef.Column.IsIdentityColumn)
                {
                    merge.Columns.Add(new AstMergeColumnNode(merge) { ColumnName = columnRef.Column.Name, ColumnUsage = MergeColumnUsage.Insert });
                }
            }

            return merge;
        }
    }
}
