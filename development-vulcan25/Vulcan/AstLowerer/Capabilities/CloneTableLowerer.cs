using System;
using System.Collections.Generic;
using System.Globalization;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Table;

namespace AstLowerer.Capabilities
{
    public static class CloneTableLowerer
    {
        private static AstTableColumnBaseNode FindColumnInClonedTable(AstTableCloneNode cloneNode, string columnName)
        {
            List<AstTableColumnBaseNode> columns = new List<AstTableColumnBaseNode>(cloneNode.Table.Columns);
            columns.AddRange(cloneNode.Columns);

            foreach (var column in columns)
            {
                if (column.Name == columnName)
                {
                    return column;
                }
            }

            return null;
        }

        public static void LowerCloneTable(IReferenceableItem item)
        {
            var cloneTableNode = item as AstTableCloneNode;
            if (cloneTableNode != null && item.FirstThisOrParent<ITemplate>() == null)
            {
                foreach (var column in cloneTableNode.Columns)
                {
                    // The IsNullable rework below is due to a bug in the Clone() function
                    // of the AST.  A later version of the AST Framework is necessary to fix this issue.

                    var preClonedColumn = FindColumnInClonedTable(cloneTableNode, column.Name);
                    if (preClonedColumn != null)
                    {
                        if (cloneTableNode.NullClonedColumns)
                        {
                            column.IsNullable = true;
                        }
                        else
                        {
                            column.IsNullable = preClonedColumn.IsNullable;
                        }
                    }
                    else
                    {
                        throw new Exception(String.Format("CloneTableLowerer: Could not find cloned column {0} in original table.", column.Name));
                    }
                    var columnTableReference = column as AstTableColumnTableReferenceNode;
                    var columnDimensionReference = column as AstTableColumnDimensionReferenceNode;
                    if (columnTableReference != null || columnDimensionReference != null)
                    {
                        var originalColumn = FindColumnInClonedTable(cloneTableNode, column.Name);
                        var originalColumnTableReference = originalColumn as AstTableColumnTableReferenceNode;
                        var originalColumnDimensionReference = originalColumn as AstTableColumnDimensionReferenceNode;
                        if (originalColumnTableReference != null && columnTableReference != null)
                        {
                            columnTableReference.Table = originalColumnTableReference.Table;
                            if (!string.IsNullOrEmpty(originalColumnTableReference.ForeignKeyNameOverride))
                            {
                                columnTableReference.ForeignKeyNameOverride = string.Empty;
                            }
                        }
                        else if (originalColumnDimensionReference != null && columnDimensionReference != null)
                        {
                            columnDimensionReference.Dimension = originalColumnDimensionReference.Dimension;
                            if (!string.IsNullOrEmpty(columnDimensionReference.ForeignKeyNameOverride))
                            {
                                columnDimensionReference.ForeignKeyNameOverride = string.Empty;
                            }
                        }

                        ((AstTableColumnTableReferenceBaseNode)column).EnforceForeignKeyConstraint = false;
                    }
                }

                if (cloneTableNode.CloneStaticSources)
                {
                    MessageEngine.Trace(cloneTableNode, Severity.Error, "CT001", "CloneStaticSources=\"true\" is not supported at this time.");
                }

                if (cloneTableNode.CloneKeys)
                {
                    int keyCount = 0;
                    foreach (var key in cloneTableNode.Table.Keys)
                    {
                        var clone = (AstTableKeyBaseNode)key.Clone();
                        clone.Name = String.Format(CultureInfo.InvariantCulture, "CTK_{0}_{1}", cloneTableNode.Name, keyCount++);
                        cloneTableNode.Keys.Add(clone);
                    }
                }

                if (cloneTableNode.CloneIndexes)
                {
                    int indexCount = 0;
                    foreach (var index in cloneTableNode.Table.Indexes)
                    {
                        var clone = (AstTableIndexNode)index.Clone();
                        clone.Name = String.Format(CultureInfo.InvariantCulture, "CTX_{0}_{1}", cloneTableNode.Name, indexCount++);
                        cloneTableNode.Indexes.Add(clone);
                    }
                }
            }
        }

        public static void ProcessCloneTable(SymbolTable symbolTable)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(symbolTable);

            foreach (var astNamedNode in snapshotSymbolTable)
            {
                LowerCloneTable(astNamedNode);
            }
        }
    }
}