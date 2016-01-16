using System;
using System.Collections.Generic;
using System.Globalization;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using VulcanEngine.IR.Ast.Task;

namespace AstLowerer.Capabilities
{
    public static class RetryContainerLowerer
    {
        public static void ProcessRetryContainers(SymbolTable symbolTable)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(symbolTable);
            foreach (var astNamedNode in snapshotSymbolTable)
            {
                var retryContainerNode = astNamedNode as AstRetryContainerTaskNode;
                if (retryContainerNode != null && astNamedNode.FirstThisOrParent<ITemplate>() == null)
                {
                    ProcessRetryContainer(retryContainerNode);
                }
            }
        }

        // TODO: Is this the right approach for events and precedence constraints?  Should we have a utility method to handle them?
        // TODO: It would be good to find an approach to unify permissions and move some of this under the securables lowerer.
        public static void ProcessRetryContainer(AstRetryContainerTaskNode retryContainerNode)
        {
            var forLoopNode = new AstForLoopContainerTaskNode(retryContainerNode.ParentItem)
                                     {
                                         Name = retryContainerNode.Name,
                                         CountingExpression = "@_retryCount=@_retryCount+1",
                                         LoopTestExpression = "@_retryCount<=@_attemptsToMake"
                                     };
            
            forLoopNode.Variables.Add(new AstVariableNode(forLoopNode)
            {
                Name = "_attemptsToMake",
                TypeCode = TypeCode.Int32,
                Value = (retryContainerNode.RetryCount - 1).ToString(CultureInfo.InvariantCulture)
            });
            forLoopNode.Variables.Add(new AstVariableNode(forLoopNode)
                                          {
                                              Name = "_retryCount",
                                              TypeCode = TypeCode.Int32,
                                              Value = "0"
                                          });
            
            forLoopNode.PrecedenceConstraints = retryContainerNode.PrecedenceConstraints;
            if (forLoopNode.PrecedenceConstraints != null)
            {
                forLoopNode.PrecedenceConstraints.ParentItem = forLoopNode;
            }

            foreach (var task in retryContainerNode.Tasks)
            {
                forLoopNode.Tasks.Add(task);
            }
            ////forLoopNode.Tasks = retryContainerNode.Tasks;
            ////if (forLoopNode.Tasks != null)
            ////{
            ////    forLoopNode.Tasks.ParentItem = forLoopNode;
            ////}

            foreach (var eventHandler in retryContainerNode.Events)
            {
                forLoopNode.Events.Add(eventHandler);
                eventHandler.ParentItem = forLoopNode;
            }

            var parentContainer = forLoopNode.ParentItem as AstContainerTaskNode;
            if (parentContainer != null)
            {
                parentContainer.Tasks.Replace(retryContainerNode, forLoopNode);
            }
        }
    }
}
