using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;

using Ssis2008Emitter.IR.TSQL;
using Ssis2008Emitter.Emitters.TSQL.SQLBuilder;

namespace Ssis2008Emitter.Emitters.TSQL.PlatformEmitter
{
    internal class MergeEmitter : PlatformEmitter
    {
        public string Emit(AstMergeTaskNode mergeTask)
        {
            StringBuilder joinBuilder = new StringBuilder();
            for (int i = 0; i < mergeTask.TargetConstraint.Columns.Count; i++)
            {
                joinBuilder.AppendFormat("TARGET.{0} = SOURCE.{0}", mergeTask.TargetConstraint.Columns[i].Column.Name);
                if (i < mergeTask.TargetConstraint.Columns.Count - 1)
                {
                    joinBuilder.AppendFormat("\nAND\n");
                }
            }

            StringBuilder notEqualBuilder = new StringBuilder();
            StringBuilder updateBuilder = new StringBuilder();
            StringBuilder insertParamBuilder = new StringBuilder();
            StringBuilder insertValueBuilder = new StringBuilder();

            AstTableNode targetTable = AstWalker.FirstParent<AstTableNode>(mergeTask.TargetConstraint);

            Hashtable columnUsageMapping = new Hashtable();
            foreach (AstTableColumnBaseNode column in targetTable.Columns)
            {
                if (column.IsAssignable)
                {
                    foreach (AstMergeColumnNode mergeColumn in mergeTask.Columns)
                    {
                        if (mergeColumn.ColumnName.ToUpperInvariant() == column.Name.ToUpperInvariant())
                        {
                            columnUsageMapping.Add(column.Name, mergeColumn.ColumnUsage);
                            break;
                        }
                    }
                    if (!columnUsageMapping.ContainsKey(column.Name))
                    {
                        columnUsageMapping.Add(column.Name, mergeTask.UnspecifiedColumnDefaultUsageType);
                    }
                }
            }

            bool firstNotEqual = true;
            bool firstUpdate = true;
            bool firstInsert = true;

            foreach (AstTableColumnBaseNode column in targetTable.Columns)
            {
                if (column.IsAssignable)
                {
                    if (columnUsageMapping[column.Name].ToString().ToUpperInvariant().Contains("COMPARE"))
                    {
                        if (firstNotEqual)
                        {
                            firstNotEqual = false;
                        }
                        else
                        {
                            notEqualBuilder.AppendFormat("\nOR\n");
                        }

                        // Bug #3757, special handling for uniqueidentifier data type
                        if (column.CustomType != null && column.CustomType.ToLowerInvariant().CompareTo("uniqueidentifier") == 0)
                        {
                            notEqualBuilder.AppendFormat("COALESCE(TARGET.[{0}],CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000')) <> COALESCE(SOURCE.[{0}],CONVERT(uniqueidentifier,'00000000-0000-0000-0000-000000000000'))", column.Name);
                        }
                        else
                        {
                            notEqualBuilder.AppendFormat("COALESCE(TARGET.[{0}],'') <> COALESCE(SOURCE.[{0}],'')", column.Name);
                        }
                    }

                    if (columnUsageMapping[column.Name].ToString().ToUpperInvariant().Contains("UPDATE"))
                    {
                        if (firstUpdate)
                        {
                            firstUpdate = false;
                        }
                        else
                        {
                            updateBuilder.AppendFormat(",");
                        }

                        updateBuilder.AppendFormat("TARGET.[{0}] = SOURCE.[{0}]", column.Name);
                    }

                    if (firstInsert)
                    {
                        firstInsert = false;
                    }
                    else
                    {
                        insertParamBuilder.AppendFormat(",\n");
                        insertValueBuilder.AppendFormat(",\n");
                    }

                    insertParamBuilder.AppendFormat("[{0}]", column.Name);
                    insertValueBuilder.AppendFormat("SOURCE.[{0}]", column.Name);
                }
            }

            TemplatePlatformEmitter te;
            if (mergeTask.UpdateTargetTable)
            {
                te = new TemplatePlatformEmitter("Merge", mergeTask.SourceName, targetTable.Name, joinBuilder.ToString(), notEqualBuilder.ToString(), updateBuilder.ToString(), insertParamBuilder.ToString(), insertValueBuilder.ToString());
            }
            else
            {
                te = new TemplatePlatformEmitter("MergeWithoutUpdate", mergeTask.SourceName, targetTable.Name, joinBuilder.ToString(), insertParamBuilder.ToString(), insertValueBuilder.ToString());
            }
            return te.Emit(null);

            /*
              <Template Name="Merge">
    <Map Source="Source" Index="0"/>
    <Map Source="Target" Index="1"/>
    <Map Source="Join" Index="2"/>
    <Map Source="NotEqualCheck" Index="3"/>
    <Map Source="Update" Index="4"/>
    <Map Source="Insert" Index="5"/>
    <TemplateData>
             */
        }
    }
}
