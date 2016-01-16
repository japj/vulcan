using System.Collections.Generic;
using AstFramework;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.IR.Framework.Connections;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Task;
using AstConnection = VulcanEngine.IR.Ast.Connection;
using AstTask = VulcanEngine.IR.Ast.Task;
using PhysicalTask = Ssis2008Emitter.IR.Tasks;

namespace Ssis2008Emitter.Phases.Lowering
{
    public static class ContainerLoweringEngine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstTask.AstPackageNode))]
        public static void LowerPackage(AstNode astNode, LoweringContext context)
        {
            var astPackage = astNode as AstTask.AstPackageNode;

            if (astPackage != null)
            {
                if (astPackage.Emit)
                {
                    var p = new Package(astPackage);
                    context.ParentObject.Children.Add(p);
                    context = new TaskLoweringContext(p);
                    LowerConnection(astPackage.LogConnection, context);
                    LowerChildren(astPackage, context);
                    LowerEventHandlers(astPackage, p, context);
                }
                else
                {
                    MessageEngine.Trace(
                        astPackage,
                        Severity.Debug,
                        "D_S008",
                        "Skipped Lowering of Package {0} because Emit was {1}",
                        astPackage.Name,
                        astPackage.Emit);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstTask.AstContainerTaskNode))]
        [Lowering(typeof(AstTask.AstStagingContainerTaskNode))]
        public static void LowerContainer(AstNode astNode, LoweringContext context)
        {
            var astContainer = astNode as AstTask.AstContainerTaskNode;

            if (astContainer != null)
            {
                var seq = new PhysicalTask.Sequence(astContainer);
                context.ParentObject.Children.Add(seq);
                LowerConnection(astContainer.LogConnection, context);

                context = new TaskLoweringContext(seq);
                LowerChildren(astContainer, context);
                LowerEventHandlers(astContainer, seq, context);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstTask.AstForLoopContainerTaskNode))]
        public static void LowerForLoop(AstNode astNode, LoweringContext context)
        {
            var astContainer = astNode as AstTask.AstForLoopContainerTaskNode;

            if (astContainer != null)
            {
                var forLoop = new PhysicalTask.ForLoop(astContainer);
                context.ParentObject.Children.Add(forLoop);
                LowerConnection(astContainer.LogConnection, context);

                context = new TaskLoweringContext(forLoop);
                LowerChildren(astContainer, context);
                LowerEventHandlers(astContainer, forLoop, context);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context", Justification = "Unused parameters required to satisfy plugin model contract.")]
        [Lowering(typeof(AstConnection.AstConnectionNode))]
        public static void LowerConnection(AstNode astNode, LoweringContext context)
        {
            var astConnection = astNode as AstConnection.AstConnectionNode;

            if (astConnection != null)
            {
                Connection c = ConnectionFactory.CreateConnection(astConnection);
                context.ParentObject.Children.Add(c);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context", Justification = "Unused parameters required to satisfy plugin model contract.")]
        [Lowering(typeof(AstTask.AstVariableNode))]
        public static void LowerVariable(AstNode astNode, LoweringContext context)
        {
            var astVariable = astNode as AstTask.AstVariableNode;
            if (astVariable != null)
            {
                var v = new Variable(astVariable);
                context.ParentObject.Children.Add(v);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void LowerEventHandlers(AstTask.AstTaskNode astTaskNode, Executable parentExecutable, LoweringContext context)
        {
            foreach (AstTask.AstTaskEventHandlerNode astTaskEventHandler in astTaskNode.Events)
            {
                PhysicalTask.EventHandler e = new IR.Tasks.EventHandler(astTaskEventHandler, parentExecutable);
                context.ParentObject.Children.Add(e);

                foreach (AstTask.AstTaskNode task in astTaskEventHandler.Tasks)
                {
                    context = new TaskLoweringContext(e);
                    PhysicalLoweringProcessor.Lower(task, context);
                }
            }
        }

        private static void LowerChildren(AstTask.AstContainerTaskNode container, LoweringContext context)
        {
            foreach (AstTask.AstVariableNode variable in container.Variables)
            {
                PhysicalLoweringProcessor.Lower(variable, context);
            }

            var sortedTasks = SortTasks(container, container.Tasks);
            foreach (AstTask.AstTaskNode task in sortedTasks)
            {
                PhysicalLoweringProcessor.Lower(task, context);
            }
        }

        private static List<AstTaskNode> SortTasks(AstTask.AstContainerTaskNode container, IEnumerable<AstTaskNode> unsortedTasks)
        {
            var roots = new HashSet<AstTaskNode>();
            var successors = new Dictionary<AstTaskNode, ICollection<AstTaskNode>>();
            ComputeRootsAndSuccessors(container, unsortedTasks, roots, successors);
            return FlowSorter.TopologicalSort(roots,successors);
        }

        private static void ComputeRootsAndSuccessors(AstTask.AstContainerTaskNode container, IEnumerable<AstTaskNode> unsortedTransformations, HashSet<AstTaskNode> roots, Dictionary<AstTaskNode, ICollection<AstTaskNode>> successors)
        {
            AstTaskNode previousTransformation = null;
            foreach (var transformation in unsortedTransformations)
            {
                HashSet<AstTaskNode> predecessors = FindPredecessors(container, transformation, previousTransformation, successors);

                if (predecessors.Count == 0)
                {
                    roots.Add(transformation);
                }

                foreach (var predecessor in predecessors)
                {
                    if (!successors.ContainsKey(predecessor))
                    {
                        successors.Add(predecessor, new HashSet<AstTaskNode>());
                    }

                    successors[predecessor].Add(transformation);
                }

                previousTransformation = transformation;
            }
        }

        private static HashSet<AstTaskNode> FindPredecessors(AstTask.AstContainerTaskNode container, AstTaskNode task, AstTaskNode previousTask, Dictionary<AstTaskNode, ICollection<AstTaskNode>> successors)
        {
            var predecessors = new HashSet<AstTaskNode>();

            if (task.PrecedenceConstraints != null)
            {
                foreach (var input in task.PrecedenceConstraints.Inputs)
                {
                    if (input.OutputPath != null)
                    {
                        var predecessorNode = input.OutputPath.ParentItem as AstTaskNode;
                        predecessors.Add(predecessorNode);
                    }
                }
            }

            //if (containerTask != null && containerTask.ConstraintMode == ContainerConstraintMode.Linear && predecessors.Count == 0 && previousTask != null)
            if (container.ConstraintMode == ContainerConstraintMode.Linear && predecessors.Count == 0 && previousTask != null)
            {
                predecessors.Add(previousTask);
            }

            return predecessors;
        }
    }
}
