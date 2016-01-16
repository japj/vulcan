using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AstFramework;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;
using AST = VulcanEngine.IR.Ast;

namespace AstLowerer.TSqlEmitter
{
    public class MergeTSqlEmitter
    {
        private readonly AST.Task.AstMergeTaskNode _mergeTask;
        private bool updateTemplateRequired;

        public MergeTSqlEmitter(AST.Task.AstMergeTaskNode astNode)
        {
            _mergeTask = astNode;
        }

        private void AppendNotEqual(AST.Table.AstTableColumnBaseNode column, StringBuilder notEqualBuilder)
        {
            if (notEqualBuilder.Length > 0)
            {
                notEqualBuilder.AppendFormat(CultureInfo.InvariantCulture, "\nOR\n");
            }

            // Bug #3757, special handling for uniqueidentifier data type
            if (column.CustomType != null && column.CustomType.Equals("uniqueidentifier", StringComparison.OrdinalIgnoreCase))
            {
                notEqualBuilder.AppendFormat("COALESCE(TARGET.[{0}],CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000')) <> COALESCE(SOURCE.[{0}],CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000'))", column.Name);
            }
            else
            {
                notEqualBuilder.AppendFormat("COALESCE(TARGET.[{0}],'') <> COALESCE(SOURCE.[{0}],'')", column.Name);
            }
        }

        private void AppendInsertValue(AST.Table.AstTableColumnBaseNode column, StringBuilder insertParamBuilder, StringBuilder insertValueBuilder)
        {
            if (insertParamBuilder.Length > 0)
            {
                insertParamBuilder.AppendFormat(CultureInfo.InvariantCulture, ",\n");
            }

            insertParamBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", column.Name);

            if (insertValueBuilder.Length > 0)
            {
                insertValueBuilder.AppendFormat(CultureInfo.InvariantCulture, ",\n");
            }

            insertValueBuilder.AppendFormat(CultureInfo.InvariantCulture, "SOURCE.[{0}]", column.Name);
        }

        private void AppendUpdate(AST.Table.AstTableColumnBaseNode column, StringBuilder updateBuilder)
        {
            updateTemplateRequired = true;
            if (updateBuilder.Length > 0)
            {
                updateBuilder.Append(",");
            }
            updateBuilder.AppendFormat(CultureInfo.InvariantCulture, "TARGET.[{0}] = SOURCE.[{0}]", column.Name);
        }

        public string Emit()
        {
            // BUGBUG - Currently ColumnName is a string in AstMergeColumnNode.  I dont want to change the binding as it would impact
            // the 2.0 to 2.5 customer ports since it would require a qualified name.
            var columnUsageMapping = new Dictionary<string, AST.Task.MergeColumnUsage>();
            foreach (AST.Task.AstMergeColumnNode mergeColumn in _mergeTask.Columns)
            {
                columnUsageMapping[mergeColumn.ColumnName.ToUpperInvariant()] = mergeColumn.ColumnUsage;
            }

            var targetTable = (AST.Table.AstTableNode)_mergeTask.TargetConstraint.ParentItem;
            var joinBuilder = new StringBuilder();
            var notEqualBuilder = new StringBuilder();
            var updateBuilder = new StringBuilder();
            var insertParamBuilder = new StringBuilder();
            var insertValueBuilder = new StringBuilder();
            bool hasIdentityColumn = false;

            for (int i = 0; i < _mergeTask.TargetConstraint.Columns.Count; i++)
            {
                joinBuilder.AppendFormat("TARGET.[{0}] = SOURCE.[{0}]", _mergeTask.TargetConstraint.Columns[i].Column.Name);
                if (i < _mergeTask.TargetConstraint.Columns.Count - 1)
                {
                    joinBuilder.AppendLine("\nAND");
                }
            }

            foreach (AST.Table.AstTableColumnBaseNode column in targetTable.Columns)
            {
                if (!columnUsageMapping.ContainsKey(column.Name.ToUpperInvariant()))
                {
                    // - Identity Columns, unless explicitly handled in the Usage Mapping, should be excluded from merge.
                    if (column.IsIdentityColumn)
                    {
                        columnUsageMapping[column.Name.ToUpperInvariant()] = AST.Task.MergeColumnUsage.Exclude;
                    }
                    else
                    {
                        columnUsageMapping[column.Name.ToUpperInvariant()] = _mergeTask.UnspecifiedColumnDefaultUsageType;
                    }
                }

                if (column.IsIdentityColumn)
                {
                    switch (columnUsageMapping[column.Name.ToUpperInvariant()])
                    {
                        case MergeColumnUsage.CompareInsert:
                            hasIdentityColumn = true;
                            break;
                        case MergeColumnUsage.CompareUpdateInsert:
                            hasIdentityColumn = true;
                            break;
                        case MergeColumnUsage.Insert:
                            hasIdentityColumn = true;
                            break;
                        case MergeColumnUsage.UpdateInsert:
                            hasIdentityColumn = true;
                            break;
                        default:
                            break;
                    }
                }

                if (column.IsAssignable || column.IsIdentityColumn)
                {
                    if (columnUsageMapping.ContainsKey(column.ToString().ToUpperInvariant()))
                    {
                        ApplyColumnUsageMapping(columnUsageMapping, column, notEqualBuilder, updateBuilder, insertParamBuilder, insertValueBuilder);
                    }
                    else
                    {
                        MessageEngine.Trace(_mergeTask, AstFramework.Severity.Error, "SSISMT001", "Column {0} does not exist in the target table and could not be mapped.", column.Name);
                    }
                }
            }

            // Complete NotEqualBuilder
            string notEqualString = notEqualBuilder.Length > 0 ? String.Format(CultureInfo.InvariantCulture, "\nAND\n(\n{0}\n)", notEqualBuilder.ToString()) : string.Empty;
            string queryHints = _mergeTask.TableLock ? "WITH(TABLOCK)" : String.Empty;

            TemplatePlatformEmitter te;
            if (updateTemplateRequired)
            {
                te = new TemplatePlatformEmitter("Merge", _mergeTask.SourceTable.SchemaQualifiedName, targetTable.SchemaQualifiedName, joinBuilder.ToString(), notEqualString, updateBuilder.ToString(), insertParamBuilder.ToString(), insertValueBuilder.ToString(),queryHints);
            }
            else
            {
                te = new TemplatePlatformEmitter("MergeWithoutUpdate", _mergeTask.SourceTable.SchemaQualifiedName, targetTable.SchemaQualifiedName, joinBuilder.ToString(), insertParamBuilder.ToString(), insertValueBuilder.ToString(),queryHints);
            }

            var finalBuilder = new StringBuilder();
            if (hasIdentityColumn)
            {
                finalBuilder.AppendFormat("\nSET IDENTITY_INSERT {0} ON\n", targetTable.SchemaQualifiedName);
            }

            finalBuilder.AppendLine(te.Emit());

            if (hasIdentityColumn)
            {
                finalBuilder.AppendFormat("\nSET IDENTITY_INSERT {0} OFF\n", targetTable.SchemaQualifiedName);
            }

            return finalBuilder.ToString();
        }

        private void ApplyColumnUsageMapping(Dictionary<string, MergeColumnUsage> columnUsageMapping, AstTableColumnBaseNode column, StringBuilder notEqualBuilder, StringBuilder updateBuilder, StringBuilder insertParamBuilder, StringBuilder insertValueBuilder)
        {
            var usage = columnUsageMapping[column.ToString().ToUpperInvariant()];
            bool compare = false;
            bool update = false;
            bool insert = false;

            switch (usage)
            {
                // Explicit truth table for safety
                case VulcanEngine.IR.Ast.Task.MergeColumnUsage.Compare:
                    compare = true;
                    update = false;
                    insert = false;
                    break;
                case VulcanEngine.IR.Ast.Task.MergeColumnUsage.CompareInsert:
                    compare = true;
                    update = false;
                    insert = true;
                    break;
                case VulcanEngine.IR.Ast.Task.MergeColumnUsage.CompareUpdate:
                    compare = true;
                    update = true;
                    insert = false;
                    break;
                case VulcanEngine.IR.Ast.Task.MergeColumnUsage.CompareUpdateInsert:
                    compare = true;
                    update = true;
                    insert = true;
                    break;
                case VulcanEngine.IR.Ast.Task.MergeColumnUsage.Insert:
                    compare = false;
                    update = false;
                    insert = true;
                    break;
                case VulcanEngine.IR.Ast.Task.MergeColumnUsage.Update:
                    compare = false;
                    update = true;
                    insert = false;
                    break;
                case VulcanEngine.IR.Ast.Task.MergeColumnUsage.UpdateInsert:
                    compare = false;
                    update = true;
                    insert = true;
                    break;
                case VulcanEngine.IR.Ast.Task.MergeColumnUsage.Exclude:
                    compare = false;
                    update = false;
                    insert = false;
                    break;
                default:
                    MessageEngine.Trace(_mergeTask, Severity.Error, "SSISMT02", "MergeColumnUsage: {0} is not supported.", usage);
                    break;
            }

            if (compare)
            {
                AppendNotEqual(column, notEqualBuilder);
            }

            if (update)
            {
                AppendUpdate(column, updateBuilder);
            }

            if (insert)
            {
                AppendInsertValue(column, insertParamBuilder, insertValueBuilder);
            }
        }
    }
}