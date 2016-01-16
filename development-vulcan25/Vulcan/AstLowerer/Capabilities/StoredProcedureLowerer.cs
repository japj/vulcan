using System.Collections.Generic;
using System.Text;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using AstLowerer.TSqlEmitter;
using VulcanEngine.IR.Ast.Task;

namespace AstLowerer.Capabilities
{
    public static class StoredProcedureLowerer
    {
        public static void ProcessStoredProcedures(SymbolTable symbolTable)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(symbolTable);
            foreach (var astNamedNode in snapshotSymbolTable)
            {
                var storedProcNode = astNamedNode as AstStoredProcNode;
                if (storedProcNode != null && astNamedNode.FirstThisOrParent<ITemplate>() == null)
                {
                    ProcessStoredProcedure(storedProcNode);
                }
            }
        }

        // TODO: Is this the right approach for events and precedence constraints?  Should we have a utility method to handle them?
        // TODO: It would be good to find an approach to unify permissions and move some of this under the securables lowerer.
        public static void ProcessStoredProcedure(AstStoredProcNode storedProcNode)
        {
            var executeSqlNode = new AstExecuteSqlTaskNode(storedProcNode.ParentItem)
                                     {
                                         Name = storedProcNode.Name,
                                         ExecuteDuringDesignTime = storedProcNode.ExecuteDuringDesignTime,
                                         Connection = storedProcNode.Connection,
                                         ResultSet = ExecuteSqlResultSet.None
                                     };
            executeSqlNode.Query = new AstExecuteSqlQueryNode(executeSqlNode) { QueryType = QueryType.Standard };

            var queryBuilder = new StringBuilder(new StoredProcTSqlEmitter(storedProcNode).Emit());
            
            foreach (var permission in storedProcNode.Permissions)
            {
                var template = new TemplatePlatformEmitter("CreateStoredProcedurePermission", permission.Action.ToString(), permission.Target.ToString(), storedProcNode.Name, permission.Principal.Name);
                queryBuilder.AppendLine(template.Emit());
            }
            
            executeSqlNode.Query.Body = queryBuilder.ToString();

            executeSqlNode.PrecedenceConstraints = storedProcNode.PrecedenceConstraints;
            if (executeSqlNode.PrecedenceConstraints != null)
            {
                executeSqlNode.PrecedenceConstraints.ParentItem = executeSqlNode;
            }

            foreach (var eventHandler in storedProcNode.Events)
            {
                executeSqlNode.Events.Add(eventHandler);
                eventHandler.ParentItem = executeSqlNode;
            }

            var parentContainer = storedProcNode.ParentItem as AstContainerTaskNode;
            if (parentContainer != null)
            {
                parentContainer.Tasks.Replace(storedProcNode, executeSqlNode);
            }
        }
    }
}
