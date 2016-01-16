using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ssis2008Emitter.IR;
using Ssis2008Emitter.IR.Tasks;
using Ssis2008Emitter.IR.Tasks.Transformations;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Task;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.Phases.Lowering
{
    public static class DataflowLoweringEngine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context", Justification = "Unused parameters required to satisfy plugin model contract.")]
        [Lowering(typeof(AstEtlRootNode))]
        public static void LowerDataflow(AstNode astNode, LoweringContext context)
        {
            var astDataflow = astNode as AstEtlRootNode;
            if (astDataflow != null)
            {
                var dft = new DataflowTask(astDataflow);
                context.ParentObject.Children.Add(dft);

                var dflc = new DataflowLoweringContext(dft);
                LowerTransformations(astDataflow, dflc);
                ContainerLoweringEngine.LowerEventHandlers(astDataflow, dft, context);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Derived type is used for making static guarantees.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void LowerTransformations(AstEtlRootNode astDataflow, DataflowLoweringContext context)
        {
            var sortedTransformations = SortTransformations(astDataflow.Transformations);
            foreach (AstTransformationNode astTransformation in sortedTransformations)
            {
                PhysicalLoweringProcessor.Lower(astTransformation, context);
            }
        }
        private static List<AstTransformationNode> SortTransformations(IEnumerable<AstTransformationNode> unsortedTransformations)
        {
            var roots = new HashSet<AstTransformationNode>();
            var successors = new Dictionary<AstTransformationNode, ICollection<AstTransformationNode>>();
            ComputeRootsAndSuccessors(unsortedTransformations, roots, successors);

            return FlowSorter.TopologicalSort(roots,successors);
        }

        private static void ComputeRootsAndSuccessors(IEnumerable<AstTransformationNode> unsortedTransformations, HashSet<AstTransformationNode> roots, Dictionary<AstTransformationNode, ICollection<AstTransformationNode>> successors)
        {
            AstTransformationNode previousTransformation = null;
            foreach (var transformation in unsortedTransformations)
            {
                HashSet<AstTransformationNode> predecessors = FindPredecessors(transformation, previousTransformation);

                if (predecessors.Count == 0)
                {
                    roots.Add(transformation);
                }

                foreach (var predecessor in predecessors)
                {
                    if (!successors.ContainsKey(predecessor))
                    {
                        successors.Add(predecessor, new HashSet<AstTransformationNode>());
                    }

                    successors[predecessor].Add(transformation);
                }

                previousTransformation = transformation;
            }
        }

        private static HashSet<AstTransformationNode> FindPredecessors(AstTransformationNode transformation, AstTransformationNode previousTransformation)
        {
            var predecessors = new HashSet<AstTransformationNode>();

            var multipleIn = transformation as AstMultipleInTransformationNode;
            if (multipleIn != null)
            {
                foreach (var inputPath in multipleIn.InputPaths)
                {
                    var predecessorNode = inputPath.OutputPath.ParentItem as AstTransformationNode;
                    predecessors.Add(predecessorNode);
                }
            }

            var singleIn = transformation as AstSingleInTransformationNode;
            if (singleIn != null && singleIn.InputPath != null && singleIn.InputPath.OutputPath != null)
            {
                var predecessorNode = singleIn.InputPath.OutputPath.ParentItem as AstTransformationNode;
                if (predecessorNode != null)
                {
                    predecessors.Add(predecessorNode);
                }
            }

            if (predecessors.Count == 0 && previousTransformation != null)
            {
                bool foundOutputPath = false;
                foreach (var outputPath in previousTransformation.StaticOutputPaths)
                {
                    if (outputPath.References.Count > 0)
                    {
                        foundOutputPath = true;
                        break;
                    }
                }

                if (!foundOutputPath)
                {
                    predecessors.Add(previousTransformation);
                }
            }

            return predecessors;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstTermLookupNode))]
        public static void LowerTermLookup(AstNode astNode, LoweringContext context)
        {
            TermLookup.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstLookupNode))]
        public static void LowerLookup(AstNode astNode, LoweringContext context)
        {
            Lookup.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstDerivedColumnListNode))]
        public static void LowerDerivedColumn(AstNode astNode, LoweringContext context)
        {
            DerivedColumns.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstSlowlyChangingDimensionNode))]
        public static void LowerSlowlyChangingDimension(AstNode astNode, LoweringContext context)
        {
            SlowlyChangingDimension.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstMulticastNode))]
        public static void LowerMulticast(AstNode astNode, LoweringContext context)
        {
            Multicast.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstConditionalSplitNode))]
        public static void LowerConditionalSplit(AstNode astNode, LoweringContext context)
        {
            ConditionalSplit.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstUnionAllNode))]
        public static void LowerUnionAll(AstNode astNode, LoweringContext context)
        {
            Union.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstDestinationNode))]
        public static void LowerDestination(AstNode astNode, LoweringContext context)
        {
            OleDBDestination.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstSortNode))]
        public static void LowerSort(AstNode astNode, LoweringContext context)
        {
            Sort.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstRowCountNode))]
        public static void LowerRowCount(AstNode astNode, LoweringContext context)
        {
            RowCount.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstOleDBCommandNode))]
        public static void LowerOleDBCommand(AstNode astNode, LoweringContext context)
        {
            OleDBCommand.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstXmlSourceNode))]
        public static void LowerXmlSource(AstNode astNode, LoweringContext context)
        {
            XmlSource.CreateAndRegister(astNode, context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        [Lowering(typeof(AstQuerySourceNode))]
        public static void LowerOleDBSource(AstNode astNode, LoweringContext context)
        {
            OleDBSource.CreateAndRegister(astNode, context);
        }
    }
}
