using System.Collections.Generic;
using System.Linq;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using Vulcan.Utility.Graph;
using VulcanEngine.AstEngine;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Transformation;

namespace AstLowerer.Capabilities
{
    public static class EtlFragmentsLowerer
    {
        public static void ProcessEtlFragments(SymbolTable symbolTable) ////HashSet<AstEtlRootNode> astEtlRootNodes)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(symbolTable);
            foreach (var astNamedNode in snapshotSymbolTable)
            {
                var fragmentReference = astNamedNode as AstEtlFragmentReferenceNode;
                if (fragmentReference != null && astNamedNode.FirstThisOrParent<ITemplate>() == null)
                {
                    AstLowererValidation.ValidateEtlFragment(fragmentReference.EtlFragment);
                    AstLowererValidation.ValidateEtlFragmentReference(fragmentReference);

                    var clonedFragment = fragmentReference.EtlFragment.Clone() as AstEtlFragmentNode;
                    var fragmentGraph = new TransformationGraph(clonedFragment.Transformations);

                    GraphNode<AstTransformationNode> sourceNode = fragmentGraph.RootNodes.FirstOrDefault(node => !(node.Item is AstSourceTransformationNode));
                    GraphNode<AstTransformationNode> sinkNode = fragmentGraph.RootNodes.FirstOrDefault(node => !(node.Item is AstSourceTransformationNode));

                    Utility.Replace(fragmentReference, clonedFragment.Transformations);
                    var etlGraph = new TransformationGraph(Utility.GetParentTransformationCollection(fragmentReference));

                    if (sourceNode != null)
                    {
                        GraphNode<AstTransformationNode> etlSourceNode = etlGraph.FindNode(sourceNode.Item);
                        AstSingleInTransformationNode source = etlSourceNode.Item as AstSingleInTransformationNode;
                        if (source != null && etlSourceNode.IncomingEdges.Count == 1)
                        {
                            source.InputPath = new AstDataflowMappedInputPathNode(source);
                            if (fragmentReference.InputPath != null)
                            {
                                source.InputPath.OutputPath = fragmentReference.InputPath.OutputPath;
                            }
                            else if (etlSourceNode.IncomingEdges.Count == 1 && etlSourceNode.IncomingEdges[0].Source.Item.PreferredOutputPath != null)
                            {
                                source.InputPath.OutputPath = etlSourceNode.IncomingEdges[0].Source.Item.PreferredOutputPath;
                            }
                            else
                            {
                                MessageEngine.Trace(fragmentReference, Severity.Error, "V0136", "Cannot find output path to bind to the root node of Etl Fragment {0}", fragmentReference.Name);
                            }

                            foreach (var inputMapping in fragmentReference.Inputs)
                            {
                                var currentMapping = new AstDataflowColumnMappingNode(source.InputPath)
                                                         {
                                                             SourceName = inputMapping.SourcePathColumnName,
                                                             TargetName = inputMapping.DestinationPathColumnName,
                                                         };
                                source.InputPath.Mappings.Add(currentMapping);
                            }
                        }
                    }

                    if (sinkNode != null)
                    {
                        GraphNode<AstTransformationNode> etlSinkNode = etlGraph.FindNode(sinkNode.Item);

                        if (etlSinkNode.OutgoingEdges.Count > 1)
                        {
                            MessageEngine.Trace(fragmentReference, Severity.Error, "V0136", "Sink nodes of Etl Fragment {0} can only have a single outgoing edge", fragmentReference.Name);
                        }

                        if (etlSinkNode.OutgoingEdges.Count > 0)
                        {
                            AstSingleInTransformationNode successor = etlSinkNode.OutgoingEdges[0].Sink.Item as AstSingleInTransformationNode;
                            if (successor != null)
                            {
                                ProcessSuccessor(fragmentReference, clonedFragment, etlSinkNode, successor);
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessSuccessor(AstEtlFragmentReferenceNode fragmentReference, AstEtlFragmentNode clonedFragment, GraphNode<AstTransformationNode> etlSinkNode, AstSingleInTransformationNode successor)
        {
            if (successor.InputPath == null)
            {
                successor.InputPath = new AstDataflowMappedInputPathNode(successor);
                successor.InputPath.OutputPath = etlSinkNode.Item.PreferredOutputPath;
            }

            // TODO:
            ////successor.InputPath.OutputPath = etlSinkNode.Item.OutputPath;

            foreach (var outputMapping in fragmentReference.Outputs)
            {
                var currentMapping = new AstDataflowColumnMappingNode(successor.InputPath)
                                         {
                                             SourceName = outputMapping.SourcePathColumnName,
                                             TargetName = outputMapping.DestinationPathColumnName
                                         };
                successor.InputPath.Mappings.Add(currentMapping);
            }

            foreach (var inputMapping in fragmentReference.Inputs)
            {
                var currentMapping = new AstDataflowColumnMappingNode(successor.InputPath)
                                         {
                                             SourceName = inputMapping.DestinationPathColumnName,
                                             TargetName = inputMapping.SourcePathColumnName
                                         };
                successor.InputPath.Mappings.Add(currentMapping);
            }

            foreach (var ignore in clonedFragment.Ignores)
            {
                var currentMapping = new AstDataflowColumnMappingNode(successor.InputPath)
                                         {
                                             SourceName = ignore.PathColumnName
                                         };
                successor.InputPath.Mappings.Add(currentMapping);
            }
        }
    }
}
