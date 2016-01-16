using System.Collections.Generic;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;
using MergeTSqlEmitter = AstLowerer.TSqlEmitter.MergeTSqlEmitter;

namespace AstLowerer.Capabilities
{
    public static class MergeLowerer
    {
        public static void ProcessMerges(SymbolTable symbolTable)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(symbolTable);
            foreach (var astNamedNode in snapshotSymbolTable)
            {
                var mergeNode = astNamedNode as AstMergeTaskNode;
                if (mergeNode != null && astNamedNode.FirstThisOrParent<ITemplate>() == null)
                {
                    ProcessMerge(mergeNode);
                }
            }
        }

        // TODO: Is this the right approach for events and precedence constraints?  Should we have a utility method to handle them?
        public static void ProcessMerge(AstMergeTaskNode mergeNode)
        {
            var executeSqlNode = new AstExecuteSqlTaskNode(mergeNode.ParentItem)
                                     {
                                         Name = mergeNode.Name,
                                         ExecuteDuringDesignTime = false,
                                         Connection = ((AstTableNode)mergeNode.TargetConstraint.ParentItem).Connection,
                                         ResultSet = ExecuteSqlResultSet.None,
                                         DelayValidation = mergeNode.DelayValidation,
                                         IsolationLevel = mergeNode.IsolationLevel
                                     };
            executeSqlNode.Query = new AstExecuteSqlQueryNode(executeSqlNode) { QueryType = QueryType.Standard, Body = new MergeTSqlEmitter(mergeNode).Emit() };

            executeSqlNode.PrecedenceConstraints = mergeNode.PrecedenceConstraints;
            if (executeSqlNode.PrecedenceConstraints != null)
            {
                executeSqlNode.PrecedenceConstraints.ParentItem = executeSqlNode;
            }

            foreach (var eventHandler in mergeNode.Events)
            {
                executeSqlNode.Events.Add(eventHandler);
                eventHandler.ParentItem = executeSqlNode;
            }

            var parentContainer = mergeNode.ParentItem as AstContainerTaskNode;
            if (parentContainer != null)
            {
                parentContainer.Tasks.Replace(mergeNode, executeSqlNode);
            }
        }
    }
}
