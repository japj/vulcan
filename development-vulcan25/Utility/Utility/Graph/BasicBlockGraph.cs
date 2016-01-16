using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Vulcan.Utility.Graph.DefaultManagement;

namespace Vulcan.Utility.Graph
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name follows the naming convention of the Graph library.")]
    public class BasicBlockGraph<T> : Graph<BasicBlock<T>>, IDisposable
    {
        public Graph<T> SourceGraph { get; private set; }

        private readonly Dictionary<GraphNode<T>, GraphNode<BasicBlock<T>>> _blockMapping;

        public BasicBlockGraph() : this(new DefaultGraphManager<BasicBlock<T>>())
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public BasicBlockGraph(IGraphManager<BasicBlock<T>> graphManager) : base(graphManager)
        {
        }

        public BasicBlockGraph(Graph<T> sourceGraph) : this(new DefaultGraphManager<BasicBlock<T>>(), sourceGraph)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public BasicBlockGraph(IGraphManager<BasicBlock<T>> graphManager, Graph<T> sourceGraph) : this(graphManager)
        {
            SourceGraph = sourceGraph;
            _blockMapping = new Dictionary<GraphNode<T>, GraphNode<BasicBlock<T>>>();

            LoadGraph(sourceGraph);

            // TODO: Remove old binding
            SourceGraph.CollectionPropertyChanged += _graph_CollectionPropertyChanged;
        }

        private void _graph_CollectionPropertyChanged(object sender, Collections.VulcanCollectionPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Nodes")
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    Nodes.Clear();
                    return;                    
                }

                if (e.OldItems != null)
                {
                    foreach (GraphNode<T> oldGraphNode in e.OldItems)
                    {
                        GraphNode<BasicBlock<T>> basicBlockNode = _blockMapping[oldGraphNode];
                        if (basicBlockNode.Item.Count > 1 || oldGraphNode.IncomingEdges.Count > 0 || oldGraphNode.OutgoingEdges.Count > 0)
                        {
                            throw new InvalidOperationException("The Graph attempted to remove a node that still had connections.");
                        }

                        _blockMapping.Remove(oldGraphNode);
                        RemoveNode(basicBlockNode);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (GraphNode<T> newGraphNode in e.NewItems)
                    {
                        var basicBlock = new BasicBlock<T> { newGraphNode.Item };
                        _blockMapping[newGraphNode] = AddNode(basicBlock);
                    }
                }
            }

            if (e.PropertyName == "Edges")
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    Edges.Clear();
                    return;
                }

                if (e.OldItems != null)
                {
                    foreach (GraphEdge<T> oldGraphEdge in e.OldItems)
                    {
                        OnSourceGraphEdgeRemoved(oldGraphEdge);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (GraphEdge<T> newGraphEdge in e.NewItems)
                    {
                        OnSourceGraphEdgeAdded(newGraphEdge);
                    }
                }
            }
        }

        private void OnSourceGraphEdgeAdded(GraphEdge<T> newGraphEdge)
        {
            GraphNode<T> source = newGraphEdge.Source;
            GraphNode<T> sink = newGraphEdge.Sink;
            if (sink.IncomingEdges.Count == 1 && source.OutgoingEdges.Count == 1)
            {
                MergeBasicBlock(_blockMapping[source], _blockMapping[sink]);
            }
            else if (sink.IncomingEdges.Count == 1 && source.OutgoingEdges.Count == 2)
            {
                var otherSourceEdge = source.OutgoingEdges[0] == newGraphEdge ? source.OutgoingEdges[1] : source.OutgoingEdges[0];
                if (otherSourceEdge.Sink.IncomingEdges.Count == 1)
                {
                    SplitBasicBlock(_blockMapping[otherSourceEdge.Source], otherSourceEdge.Sink.Item);
                }

                if (FindEdge(_blockMapping[source], _blockMapping[sink]) == null)
                {
                    OnAddEdge(_blockMapping[source], _blockMapping[sink], newGraphEdge);
                }
            }
            else if (sink.IncomingEdges.Count == 2)
            {
                var otherEdge = sink.IncomingEdges[0] == newGraphEdge ? sink.IncomingEdges[1] : sink.IncomingEdges[0];
                if (otherEdge.Source.OutgoingEdges.Count == 1)
                {
                    SplitBasicBlock(_blockMapping[otherEdge.Source], sink.Item);
                }
            }
            else
            {
                if (FindEdge(_blockMapping[source], _blockMapping[sink]) == null)
                {
                    OnAddEdge(_blockMapping[source], _blockMapping[sink], newGraphEdge);
                }
            }
        }

        private void OnSourceGraphEdgeRemoved(GraphEdge<T> oldGraphEdge)
        {
            GraphNode<T> source = oldGraphEdge.Source;
            GraphNode<T> sink = oldGraphEdge.Sink;
            if (sink.IncomingEdges.Count == 0 && source.OutgoingEdges.Count == 0)
            {
                SplitBasicBlock(_blockMapping[source], sink.Item);
            }
            else if (sink.IncomingEdges.Count == 0 && source.OutgoingEdges.Count == 1 && source.OutgoingEdges[0].Sink.IncomingEdges.Count == 1)
            {
                MergeBasicBlock(_blockMapping[source], _blockMapping[source.OutgoingEdges[0].Sink]);
            }
            else if (sink.IncomingEdges.Count == 1 && !sink.IsLeader)
            {
                MergeBasicBlock(_blockMapping[sink.IncomingEdges[0].Source], _blockMapping[sink]);
            }

            if (SourceGraph.FindEdge(source.Item, sink.Item) == null)
            {
                OnRemoveEdge(FindEdge(_blockMapping[source], _blockMapping[sink]));
            }
        }

        private void SplitBasicBlock(GraphNode<BasicBlock<T>> basicBlock, T splitPoint)
        {
            BasicBlock<T> newBasicBlock = new BasicBlock<T>();
            var newBasicBlockNode = AddNode(newBasicBlock);

            int splitIndex = basicBlock.Item.IndexOf(splitPoint);
            int i = splitIndex;
            while (i < basicBlock.Item.Count)
            {
                T item = basicBlock.Item[i];
                basicBlock.Item.RemoveAt(i);
                newBasicBlock.Add(item);
                _blockMapping[SourceGraph.FindNode(item)] = newBasicBlockNode;
            }

            var tempOutgoingEdges = new List<GraphEdge<BasicBlock<T>>>(basicBlock.OutgoingEdges);
            foreach (var outgoingEdge in tempOutgoingEdges)
            {
                OnRemoveEdge(outgoingEdge);
            }

            var leaderNode = SourceGraph.FindNode(newBasicBlock.Leader);
            var lastNode = SourceGraph.FindNode(newBasicBlock.Last);
            foreach (var edge in leaderNode.IncomingEdges)
            {
                OnAddEdge(_blockMapping[edge.Source], newBasicBlockNode, edge);
            }

            foreach (var edge in lastNode.OutgoingEdges)
            {
                OnAddEdge(newBasicBlockNode, _blockMapping[edge.Sink], edge);
            }
        }

        private void MergeBasicBlock(GraphNode<BasicBlock<T>> basicBlock1, GraphNode<BasicBlock<T>> basicBlock2)
        {
            if (basicBlock1.OutgoingEdges.Count > 0)
            {
                OnRemoveEdge(basicBlock1.OutgoingEdges[0]);
            }

            foreach (var edge in basicBlock2.OutgoingEdges)
            {
                OnAddEdge(basicBlock1, edge.Sink, edge.SourceData, edge.SinkData);
            } 

            RemoveNode(basicBlock2);
            
            foreach (var item in basicBlock2.Item)
            {
                basicBlock1.Item.Add(item);
                _blockMapping[SourceGraph.FindNode(item)] = basicBlock1;
            }
        }

        private void LoadGraph(Graph<T> graph)
        {
            var leaders = graph.Nodes.Where(node => node.IsLeader);

            var blocksByLeader = new Dictionary<GraphNode<T>, BasicBlock<T>>();
            var blocksByLast = new Dictionary<GraphNode<T>, BasicBlock<T>>();

            foreach (var leader in leaders)
            {
                var basicBlock = new BasicBlock<T> { leader.Item };

                GraphNode<T> last = leader;
                if (leader.OutgoingEdges.Count == 1)
                {
                    GraphNode<T> currentNode = leader.OutgoingEdges[0].Sink;
                    while (currentNode != null && !currentNode.IsLeader)
                    {
                        basicBlock.Add(currentNode.Item);
                        last = currentNode;
                        currentNode = currentNode.OutgoingEdges.Count == 1 ? currentNode.OutgoingEdges[0].Sink : null;
                    }
                }

                blocksByLeader[leader] = basicBlock;
                blocksByLast[last] = basicBlock;
            }

            foreach (var leaderBlockPair in blocksByLeader)
            {
                var basicBlockNode = AddNode(leaderBlockPair.Value);
                foreach (var graphNode in leaderBlockPair.Value)
                {
                    _blockMapping[graph.FindNode(graphNode)] = basicBlockNode;
                }
            }

            foreach (var leaderBlockPair in blocksByLeader)
            {
                foreach (var incomingEdge in leaderBlockPair.Key.IncomingEdges)
                {
                    OnAddEdge(blocksByLast[incomingEdge.Source], leaderBlockPair.Value, incomingEdge);
                }
            }
        }

        private void OnAddEdge(BasicBlock<T> source, BasicBlock<T> sink, GraphEdge<T> dataProvider)
        {
            AddEdge(source, sink, null, dataProvider.SourceData, dataProvider.SinkData);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        private void OnAddEdge(GraphNode<BasicBlock<T>> sourceNode, GraphNode<BasicBlock<T>> sinkNode, object sourceData, object sinkData)
        {
            AddEdge(sourceNode, sinkNode, null, sourceData, sinkData);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        private void OnAddEdge(GraphNode<BasicBlock<T>> sourceNode, GraphNode<BasicBlock<T>> sinkNode, GraphEdge<T> dataProvider)
        {
            AddEdge(sourceNode, sinkNode, null, dataProvider.SourceData, dataProvider.SinkData);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        private void OnRemoveEdge(GraphEdge<BasicBlock<T>> edge)
        {
            RemoveEdge(edge);
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (SourceGraph != null)
                {
                    SourceGraph.CollectionPropertyChanged -= _graph_CollectionPropertyChanged;
                    SourceGraph = null;
                }
            }
        }
        #endregion
    }
}
