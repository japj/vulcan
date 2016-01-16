using System;
using System.Collections.Generic;
using System.Globalization;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using AstLowerer.TSqlEmitter;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;
using Ssis2008Emitter.IR.Common;

namespace AstLowerer.Capabilities
{
    public static class StagingContainerLowerer
    {
        // TODO: We've made the call to just use the staging node as the lowered container rather than creating a new one and copying everything over
        public static void ProcessContainers(SymbolTable symbolTable)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(symbolTable);
            foreach (var astNamedNode in snapshotSymbolTable)
            {
                var stagingNode = astNamedNode as AstStagingContainerTaskNode;
                if (stagingNode != null && astNamedNode.FirstThisOrParent<ITemplate>() == null)
                {
                    var stagingCreateContainer = new AstContainerTaskNode(stagingNode)
                    {
                        Name = String.Format(CultureInfo.InvariantCulture, Properties.Resources.CreateStaging, stagingNode.Name),
                        Log = false,
                    };

                    var stagingDropContainer = new AstContainerTaskNode(stagingNode)
                    {
                        Name = String.Format(CultureInfo.InvariantCulture, Properties.Resources.DropStaging, stagingNode.Name),
                        Log = false,
                    };

                    stagingNode.Tasks.Insert(0, stagingCreateContainer);
                    stagingNode.Tasks.Add(stagingDropContainer);

                    foreach (var baseTable in stagingNode.Tables)
                    {
                        var table = baseTable as AstTableNode;
                        if (table != null)
                        {
                            TableLowerer.LowerTable(
                                stagingCreateContainer,
                                table,
                                String.Format(CultureInfo.InvariantCulture, Properties.Resources.CreateStagingTable, table.Name),
                                stagingNode.ExecuteDuringDesignTime);

                            var dropStagingTemplate = new TemplatePlatformEmitter("DropStagingTable", table.SchemaQualifiedName);
                            var dropTableExecuteSqlTask = new AstExecuteSqlTaskNode(stagingNode)
                                                              {
                                                                  Name = StringManipulation.NameCleanerAndUniqifier(String.Format(CultureInfo.InvariantCulture, Properties.Resources.DropStagingTable, table.Name)),
                                                                  Connection = table.Connection,
                                                                  ExecuteDuringDesignTime = stagingNode.ExecuteDuringDesignTime,
                                                              };
                            dropTableExecuteSqlTask.Query = new AstExecuteSqlQueryNode(dropTableExecuteSqlTask) { QueryType = QueryType.Standard, Body = dropStagingTemplate.Emit() };
                            stagingDropContainer.Tasks.Add(dropTableExecuteSqlTask);
                        }
                        else
                        {
                            throw new System.NotSupportedException("AstLowering - StagingContainer - a Table Template node was found when lowering staging containers and I don't know what to do with it.");
                        }
                    }
                }
            }
        }
    }
}
