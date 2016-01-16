using System;
using System.Linq;
using AstFramework;
using VulcanEngine.AstEngine;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Transformation;

namespace AstLowerer
{
    public static class AstLowererValidation
    {
        public static void ValidateLateArrivingTable(AstTableNode table)
        {
            bool hasIdentity = table.Keys.Any(item => item is AstTableIdentityNode);
            bool hasPrimaryKey = table.Keys.Any(item => item is AstTablePrimaryKeyNode);
            if (!hasIdentity ^ hasPrimaryKey)
            {
                MessageEngine.Trace(table, Severity.Error, "V0130", "To support Late Arriving, table {0} must provide either identity or primary key", table.Name);
            }

            bool foundEligibleKey = false;
            foreach (AstTableKeyBaseNode key in table.Keys)
            {
                if (key.Columns.Any(keyColumn => !keyColumn.Column.IsComputed))
                {
                    if (foundEligibleKey)
                    {
                        MessageEngine.Trace(table, Severity.Error, "V0131", "There can be at most one (primary, identity, unique) key that is based on non-computed columns for Late Arriving Table {0}", table.Name);
                    }

                    foundEligibleKey = true;
                }
            }

            foreach (AstTableColumnBaseNode column in table.Columns)
            {
                if (!column.IsNullable && String.IsNullOrEmpty(column.Default))
                {
                    MessageEngine.Trace(table, Severity.Error, "V0132", "Late Arriving Table {0} must have default values for all columns that are non-nullable.", table.Name);
                }
            }
        }

        public static void ValidateScdTable(AstTableNode table)
        {
            // TODO: Can we use identity or primary (or just primary)
            // TODO: Need to add check - For implicit invocation, the destination can not be an OLEDB destination.  A compile-time error will be emitted if so.
            bool hasIdentity = table.Keys.Any(item => item is AstTableIdentityNode);
            bool hasPrimaryKey = table.Keys.Any(item => item is AstTablePrimaryKeyNode);
            
            if (!(hasIdentity ^ hasPrimaryKey))
            {
                MessageEngine.Trace(table, Severity.Error, "V0133", "To support Scd Columns, table {0} must provide either identity or primary key", table.Name);
            }

            // TODO: Should we be overwriting these? And do we need to recheck the table for Scd columns after rewriting?
            foreach (var keyColumn in table.PreferredKey.Columns)
            {
                keyColumn.Column.ScdType = ScdType.Key;
            }

            foreach (AstTableColumnBaseNode column in table.Columns)
            {
                if (column.IsAssignable && column.ScdType == ScdType.Update && !column.IsNullable && String.IsNullOrEmpty(column.Default))
                {
                    MessageEngine.Trace(table, Severity.Error, "V0134", "Non-nullable ScdType.Update columns in table {0} with Error or Historical columns must have default values. Provide a default value for column {1}", table.Name, column.Name);
                }
            }
        }

        public static void ValidateLateArrivingLookup(AstLateArrivingLookupNode lookup)
        {
            if (!lookup.Table.LateArriving)
            {
                MessageEngine.Trace(lookup, Severity.Error, "V0128", "Table {0} is not designated for Late Arriving Lookup.  Set LateArriving = true.", lookup.Table.Name);
            }

            AstTableKeyBaseNode eligibleKey = null;
            foreach (AstTableKeyBaseNode key in lookup.Table.Keys)
            {
                if (key.Columns.Any(keyColumn => !keyColumn.Column.IsComputed))
                {
                    eligibleKey = key;
                    break; // Safe to do since we already verified single eligble key in the table processing pass
                }
            }

            if (eligibleKey == null)
            {
                MessageEngine.Trace(lookup, Severity.Error, "V0129", "Late Arriving Table {0} must specify an eligible key.", lookup.Table.Name);
            }

            // TODO: Finish checking this logic
            foreach (AstTableKeyColumnNode eligibleKeyColumn in eligibleKey.Columns)
            {
                if (!lookup.Inputs.Any(io => io.RemoteColumnName == eligibleKeyColumn.Column.Name))
                {
                    MessageEngine.Trace(lookup, Severity.Error, "V0123", "Late Arriving Lookup {0} must specify inputs for every column of constraint: {1}", lookup.Name, eligibleKey.Name);
                }
            }
        }

        public static void ValidateEtlFragmentReference(AstEtlFragmentReferenceNode fragmentReference)
        {
            if (fragmentReference.Inputs.Count != fragmentReference.EtlFragment.Inputs.Count)
            {
                MessageEngine.Trace(fragmentReference, Severity.Error, "V0124", "The fragment reference input mapping count does not match the exposed input count of the fragment.");
            }

            if (fragmentReference.Outputs.Count != fragmentReference.EtlFragment.Outputs.Count)
            {
                MessageEngine.Trace(fragmentReference, Severity.Error, "V0125", "The fragment reference output mapping count does not match the exposed output count of the fragment.");
            }

            foreach (var input in fragmentReference.Inputs)
            {
                if (!fragmentReference.EtlFragment.Inputs.Any(decl => decl.PathColumnName.Equals(input.DestinationPathColumnName)))
                {
                    MessageEngine.Trace(fragmentReference, Severity.Error, "V0126", "The fragment reference input column mapping with source {0} did not match the exposed input columns in the fragment.", input.SourcePathColumnName);
                }
            }

            foreach (var output in fragmentReference.Outputs)
            {
                if (!fragmentReference.EtlFragment.Outputs.Any(decl => decl.PathColumnName.Equals(output.SourcePathColumnName)))
                {
                    MessageEngine.Trace(fragmentReference, Severity.Error, "V0127", "A fragment reference output column mapping with destination {0} did not match the exposed output columns in the fragment.", output.DestinationPathColumnName);
                }
            }
        }

        public static void ValidateEtlFragment(AstEtlFragmentNode etlFragment)
        {
            var graph = new TransformationGraph(etlFragment.Transformations);

            AstTransformationNode root = null;
            foreach (var rootNode in graph.RootNodes)
            {
                if (!(rootNode.Item is AstSourceTransformationNode))
                {
                    if (root != null)
                    {
                        MessageEngine.Trace(etlFragment, Severity.Error, "V0120", "Etl Fragments cannot have more than one root node with InputPaths");
                    }

                    root = rootNode.Item;
                }
            }

            AstTransformationNode leaf = null;
            foreach (var leafNode in graph.LeafNodes)
            {
                if (!(leafNode.Item is AstDestinationNode))
                {
                    if (leaf != null)
                    {
                        MessageEngine.Trace(etlFragment, Severity.Error, "V0121", "Etl Fragments cannot have more than one leaf node with OutputPaths");
                    }

                    leaf = leafNode.Item;
                }
            }
        }
    }
}
