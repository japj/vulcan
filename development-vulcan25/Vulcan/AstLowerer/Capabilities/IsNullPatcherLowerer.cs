using System;
using System.Collections.Generic;
using System.Globalization;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using VulcanEngine.IR.Ast.Transformation;

namespace AstLowerer.Capabilities
{
    public static class IsNullPatcherLowerer
    {
        public static void ProcessIsNullPatcherTransformations(SymbolTable symbolTable)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(symbolTable);
            foreach (var astNamedNode in snapshotSymbolTable)
            {
                var nullPatcherNode = astNamedNode as AstIsNullPatcherNode;
                if (nullPatcherNode != null && astNamedNode.FirstThisOrParent<ITemplate>() == null)
                {
                    var astDerivedColumnListNode = new AstDerivedColumnListNode(nullPatcherNode.ParentItem)
                                                       {
                                                           Name = nullPatcherNode.Name,
                                                           ValidateExternalMetadata = nullPatcherNode.ValidateExternalMetadata
                                                       };

                    foreach (AstIsNullPatcherColumnNode patchColumn in nullPatcherNode.Columns)
                    {
                        var column = new AstDerivedColumnNode(astDerivedColumnListNode)
                                         {
                                             Name = patchColumn.Name,
                                             ReplaceExisting = true,
                                             Expression = String.Format(CultureInfo.InvariantCulture, "ISNULL({0}) ? {1} : {0}", patchColumn.Name, patchColumn.DefaultValue),
                                             DerivedColumnType = VulcanEngine.IR.Ast.ColumnType.Object
                                         };
                        astDerivedColumnListNode.Columns.Add(column);
                    }

                    Utility.Replace(nullPatcherNode, new List<AstTransformationNode> { astDerivedColumnListNode });
                }
            }
        }
    }
}