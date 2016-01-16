using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Vulcan.Utility.ComponentModel;

namespace Vulcan.Utility.Graph
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name follows the naming convention of the Graph library.")]
    public class Graph<T> : VulcanNotifyPropertyChanged, IEnumerable<T>
    {
        public IGraphManager<T> GraphManager { get; private set; }

        // TODO: We need the concept of a readonly VulcanCollection!  Every one of these collections should use it
        public GraphNodeCollection<T> Nodes { get; private set; }

        public GraphEdgeCollection<T> Edges { get; private set; }

        public GraphNodeCollection<T> RootNodes { get; private set; }

        public GraphNodeCollection<T> LeafNodes { get; private set; }

        private bool _isInUpdate;

        private bool _isObservable;

        public bool IsObservable
        {
            get { return _isObservable; }
            set
            {
                if (_isObservable != value)
                {
                    _isObservable = value;
                    OnGraphChanged();
                }
            }
        }

        private bool _isDirty;

        public bool PermitDuplicateNodeItems { get; set; }

        public bool ErrorOnDuplicateNodeAddAttempt { get; set; }

        public Graph() : this(new DefaultManagement.DefaultGraphManager<T>())
        {
        }

        public Graph(IGraphManager<T> graphManager)
        {
            GraphManager = graphManager;

            Nodes = new GraphNodeCollection<T>(this);
            Nodes.CollectionChanged += Nodes_CollectionChanged;
            
            Edges = new GraphEdgeCollection<T>(this);
            Edges.CollectionChanged += Edges_CollectionChanged;

            RootNodes = new GraphNodeCollection<T>(this);
            RootNodes.CollectionChanged += (sender, e) => VulcanOnCollectionPropertyChanged("RootNodes", e);

            LeafNodes = new GraphNodeCollection<T>(this);
            LeafNodes.CollectionChanged += (sender, e) => VulcanOnCollectionPropertyChanged("LeafNodes", e);
        }

        public void EnsureUpdated()
        {
            if (_isDirty && !_isInUpdate)
            {
                _isInUpdate = true;
                UpdateDominators();
                UpdateDominanceFrontiers();
                _isDirty = false;
                _isInUpdate = false;
            }
        }

        protected void OnGraphChanged()
        {
            _isDirty = true;
            if (_isObservable)
            {
                EnsureUpdated();
            }
        }

        #region Node Management
        private void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VulcanOnCollectionPropertyChanged("Nodes", e);
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                throw new NotImplementedException();
            }

            if (e.OldItems != null)
            {
                foreach (GraphNode<T> oldNode in e.OldItems)
                {
                    OnRemoveNode(oldNode);
                }
            }

            if (e.NewItems != null)
            {
                foreach (GraphNode<T> newNode in e.NewItems)
                {
                    OnAddNode(newNode);
                }
            }

            OnGraphChanged();
        }

        private void OnAddNode(GraphNode<T> node)
        {
            RootNodes.Add(node);
            LeafNodes.Add(node);
        }

        private void OnRemoveNode(GraphNode<T> node)
        {
            RootNodes.Remove(node);
            var tempIncomingEdges = new List<GraphEdge<T>>(node.IncomingEdges);
            foreach (GraphEdge<T> incomingEdge in tempIncomingEdges)
            {
                RemoveEdge(incomingEdge);
            }

            LeafNodes.Remove(node);
            var tempOutgoingEdges = new List<GraphEdge<T>>(node.OutgoingEdges);
            foreach (GraphEdge<T> outgoingEdge in tempOutgoingEdges)
            {
                RemoveEdge(outgoingEdge);
            }
        }
        #endregion Node Management

        #region Edge Management
        private void Edges_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                throw new NotImplementedException();
            }

            if (e.OldItems != null)
            {
                foreach (GraphEdge<T> oldEdge in e.OldItems)
                {
                    OnRemoveEdge(oldEdge);
                }
            }

            if (e.NewItems != null)
            {
                foreach (GraphEdge<T> newEdge in e.NewItems)
                {
                    OnAddEdge(newEdge);
                }
            }

            OnGraphChanged();
            VulcanOnCollectionPropertyChanged("Edges", e);
        }

        private void OnAddEdge(GraphEdge<T> edge)
        {
            if (edge.Source.IsLeafNode)
            {
                LeafNodes.Remove(edge.Source);
            }

            if (edge.Sink.IsRootNode)
            {
                RootNodes.Remove(edge.Sink);
            }

            edge.Source.OutgoingEdges.Add(edge);
            edge.Sink.IncomingEdges.Add(edge);
        }

        private void OnRemoveEdge(GraphEdge<T> edge)
        {
            edge.Sink.IncomingEdges.Remove(edge);
            edge.Source.OutgoingEdges.Remove(edge);

            if (edge.Sink.IsRootNode)
            {
                RootNodes.Add(edge.Sink);
            }

            if (edge.Source.IsLeafNode)
            {
                LeafNodes.Add(edge.Source);
            }
        }
        #endregion Edge Management

        #region Finders
        public GraphNode<T> FindNode(T item)
        {
            var targets = Nodes.Where(currentNode => currentNode.Item.Equals(item)).ToList();

            if (targets.Count == 1)
            {
                return targets[0];
            }

            return null;
        }

        public GraphEdge<T> FindEdge(T source, T sink)
        {
            var targets = Edges.Where(currentEdge => currentEdge.Source.Item.Equals(source) && currentEdge.Sink.Item.Equals(sink)).ToList();

            if (targets.Count == 1)
            {
                return targets[0];
            }

            // TODO: Should this return the first match for basic block support?  Might hide bugs...
            return null;
        }

        public GraphEdge<T> FindEdge(T source, T sink, object sourceData, object sinkData)
        {
            var targets = Edges.Where(currentEdge => currentEdge.Source.Item.Equals(source) && currentEdge.Sink.Item.Equals(sink) && currentEdge.SourceData == sourceData && currentEdge.SinkData == sinkData).ToList();

            if (targets.Count == 1)
            {
                return targets[0];
            }

            return null;
        }

        public GraphEdge<T> FindEdge(GraphNode<T> sourceNode, GraphNode<T> sinkNode)
        {
            var targets = Edges.Where(currentEdge => currentEdge.Source.Equals(sourceNode) && currentEdge.Sink.Equals(sinkNode));

            if (targets.Count() == 1)
            {
                return targets.First();
            }

            return null;
        }
        #endregion Finders

        // TODO: Ideally we would create a new collection type that delegated before adding to the collection, rather than after.  That way we would avoid these methods.
        #region Graph Management Delegation
        public GraphNode<T> AddNode(T item)
        {
            return GraphManager.GraphNodeBuilder.AddNode(this, item);
        }
        
        public bool RemoveNode(GraphNode<T> node)
        {
            return GraphManager.GraphNodeRemover.RemoveNode(this, node);
        }

        public GraphEdge<T> AddEdge(T source, T sink)
        {
            return AddEdge(source, sink, null, null, null);
        }

        public GraphEdge<T> AddEdge(T source, T sink, string label, object sourceData, object sinkData)
        {
            var sourceCandidates = new List<GraphNode<T>>();
            var sinkCandidates = new List<GraphNode<T>>();
            foreach (var node in Nodes)
            {
                if (node.Item.Equals(source))
                {
                    sourceCandidates.Add(node);
                }

                if (node.Item.Equals(sink))
                {
                    sinkCandidates.Add(node);
                }
            }

            if (sourceCandidates.Count > 1 || sinkCandidates.Count > 1)
            {
                return null;
            }

            GraphNode<T> sourceNode = sourceCandidates.Count == 0 ? AddNode(source) : sourceCandidates[0];
            GraphNode<T> sinkNode = sinkCandidates.Count == 0 ? AddNode(sink) : sinkCandidates[0];
            return AddEdge(sourceNode, sinkNode, label, sourceData, sinkData);
        }

        public GraphEdge<T> AddEdge(GraphNode<T> sourceNode, GraphNode<T> sinkNode)
        {
            return AddEdge(sourceNode, sinkNode, null, null, null);
        }

        public GraphEdge<T> AddEdge(GraphNode<T> source, GraphNode<T> sink, string label, object sourceData, object sinkData)
        {
            return AddEdge(new GraphEdge<T>(source, sink, label, sourceData, sinkData));
        }

        public GraphEdge<T> AddEdge(GraphEdge<T> edge)
        {
            GraphManager.GraphEdgeBuilder.AddEdge(this, edge);
            return edge;
        }

        public bool RemoveEdge(GraphEdge<T> edge)
        {
            return GraphManager.GraphEdgeRemover.RemoveEdge(this, edge);
        }

        public void RelocateEdge(GraphEdge<T> edge, GraphNode<T> newSourceNode, GraphNode<T> newSinkNode)
        {
            GraphManager.GraphEdgeRelocator.RelocateEdge(this, edge, newSourceNode, newSinkNode);
        }
        #endregion Graph Management Delegation

        #region Enumerators
        public IEnumerable<T> DepthFirstSearch
        {
            get { return new GraphEnumerable<T>(this, GraphSearchAlgorithm.DepthFirstSearch); }
        }

        public IEnumerable<T> BreadthFirstSearch
        {
            get { return new GraphEnumerable<T>(this, GraphSearchAlgorithm.BreadthFirstSearch); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return DepthFirstSearch.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion Enumerators
        
        private IEnumerable<GraphNode<T>> ReverseTopologicalSortNodes()
        {
            var sorted = new List<GraphNode<T>>();
            var queue = new Queue<GraphNode<T>>(Nodes.Where(node => node.DominatedBy.Count == 1));
            var visited = new HashSet<GraphNode<T>>(queue);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                sorted.Add(current);
                foreach (var dominated in current.ImmediatelyDominates)
                {
                    if (!visited.Contains(dominated))
                    {
                        queue.Enqueue(dominated);
                        visited.Add(dominated);
                    }
                }
            }

            sorted.Reverse();
            return sorted;
        }
        
        private void UpdateDominanceFrontiers()
        {
            var dominanceFrontierDictionary = new Dictionary<GraphNode<T>, HashSet<GraphNode<T>>>();
            var dominanceFrontierOfDictionary = new Dictionary<GraphNode<T>, HashSet<GraphNode<T>>>();
            foreach (var node in Nodes)
            {
                dominanceFrontierDictionary.Add(node, new HashSet<GraphNode<T>>());
                dominanceFrontierOfDictionary.Add(node, new HashSet<GraphNode<T>>());
            }

            var reverseTopological = Nodes.ToList();
            reverseTopological.Sort((a, b) => a == b ? 0 : a.Dominates.Contains(b) ? -1 : b.Dominates.Contains(a) ? 1 : 0);

            foreach (var currentNode in ReverseTopologicalSortNodes())
            {
                foreach (var immediateSuccessor in currentNode.ImmediateSuccessors)
                {
                    if (immediateSuccessor.ImmediatelyDominatedBy != currentNode)
                    {
                        dominanceFrontierDictionary[currentNode].Add(immediateSuccessor);
                        dominanceFrontierOfDictionary[immediateSuccessor].Add(currentNode);
                    }
                }

                foreach (var immediatelyDominated in currentNode.ImmediatelyDominates)
                {
                    foreach (var dominanceFrontierNode in dominanceFrontierDictionary[immediatelyDominated])
                    {
                        if (dominanceFrontierNode.ImmediatelyDominatedBy != currentNode)
                        {
                            dominanceFrontierDictionary[currentNode].Add(dominanceFrontierNode);
                            dominanceFrontierOfDictionary[dominanceFrontierNode].Add(currentNode);
                        }
                    }
                }
            }

            foreach (var node in Nodes)
            {
                DestructiveSyncHashSets(dominanceFrontierDictionary[node], node.DominanceFrontier);
                DestructiveSyncHashSets(dominanceFrontierOfDictionary[node], node.DominanceFrontierOf);
            }
        }

        private void UpdateDominators()
        {
            Dictionary<GraphNode<T>, HashSet<GraphNode<T>>> nodeDominatedBy = CalculateDominators();

            var nodeDominates = new Dictionary<GraphNode<T>, HashSet<GraphNode<T>>>();
            var nodeImmediatelyDominates = new Dictionary<GraphNode<T>, HashSet<GraphNode<T>>>();
            foreach (var node in Nodes)
            {
                nodeDominates.Add(node, new HashSet<GraphNode<T>> { node });
                nodeImmediatelyDominates.Add(node, new HashSet<GraphNode<T>>());
            }

            foreach (var node in Nodes)
            {
                int maxDominatorCount = Int32.MinValue;
                GraphNode<T> immediateDominator = null;
                foreach (var dominator in nodeDominatedBy[node])
                {
                    nodeDominates[dominator].Add(node);
                    if (dominator != node && nodeDominatedBy[dominator].Count > maxDominatorCount)
                    {
                        maxDominatorCount = nodeDominatedBy[dominator].Count;
                        immediateDominator = dominator;
                    }
                }

                if (immediateDominator != null)
                {
                    nodeImmediatelyDominates[immediateDominator].Add(node);
                    node.ImmediatelyDominatedBy = immediateDominator;
                }
            }

            foreach (var node in Nodes)
            {
                DestructiveSyncHashSets(nodeDominatedBy[node], node.DominatedBy);
                DestructiveSyncHashSets(nodeDominates[node], node.Dominates);
                DestructiveSyncHashSets(nodeImmediatelyDominates[node], node.ImmediatelyDominates);
            }
        }

        private Dictionary<GraphNode<T>, HashSet<GraphNode<T>>> CalculateDominators()
        {
            var nodeDominatedBy = new Dictionary<GraphNode<T>, HashSet<GraphNode<T>>>();
            foreach (var node in Nodes)
            {
                nodeDominatedBy.Add(node, new HashSet<GraphNode<T>>(Nodes));
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var node in Nodes)
                {
                    int lastCount = nodeDominatedBy[node].Count;
                    if (node.IncomingEdges.Count == 0)
                    {
                        nodeDominatedBy[node].Clear();
                    }

                    foreach (var incomingEdge in node.IncomingEdges)
                    {
                        nodeDominatedBy[node].IntersectWith(nodeDominatedBy[incomingEdge.Source]);
                    }

                    nodeDominatedBy[node].Add(node);
                    changed = changed || nodeDominatedBy[node].Count != lastCount;
                }
            }

            return nodeDominatedBy;
        }

        private static bool DestructiveSyncHashSets<TItem>(HashSet<TItem> source, ICollection<TItem> target)
        {
            bool changeMade = false;
            source.SymmetricExceptWith(target);
            foreach (var difference in source)
            {
                changeMade = true;
                if (!target.Remove(difference))
                {
                    target.Add(difference);
                }
            }

            return changeMade;
        }

        #region Cyclicality Checker
        public bool IsAcyclic
        {
            get
            {
                // TODO: Cache result with a dirty bit
                // TODO: Can we do this iteratively?  Perhaps we can leverage dominator analysis on edge modifications?
                var visitedPhaseInProgress = new Dictionary<GraphNode<T>, bool>();

                if (Nodes.Count > 0 && RootNodes.Count == 0)
                {
                    return false;
                }

                foreach (var rootNode in RootNodes)
                {
                    var dfsHostStack = new Stack<GraphNode<T>>();
                    dfsHostStack.Push(rootNode);
                    while (dfsHostStack.Count > 0)
                    {
                        var currentNode = dfsHostStack.Pop();

                        if (currentNode.IsLeafNode)
                        {
                            visitedPhaseInProgress[currentNode] = false;
                        }
                        else if (!visitedPhaseInProgress.ContainsKey(currentNode))
                        {
                            visitedPhaseInProgress.Add(currentNode, true);
                            dfsHostStack.Push(currentNode);  // Pushing back ensures that it gets completed after children are completed.
                            foreach (var outgoingEdge in currentNode.OutgoingEdges)
                            {
                                dfsHostStack.Push(outgoingEdge.Sink);
                            }
                        }
                        else if (visitedPhaseInProgress[currentNode])
                        {
                            foreach (var outgoingEdge in currentNode.OutgoingEdges)
                            {
                                // REVIEW: Check order of execution rules on   can the indexer generate an exception?
                                if (!visitedPhaseInProgress.ContainsKey(outgoingEdge.Sink) || visitedPhaseInProgress[outgoingEdge.Sink])
                                {
                                    return false;
                                }
                            }

                            visitedPhaseInProgress[currentNode] = false;
                        }
                        //// else { No need to re-inspect completed subgraphs }
                    }
                }

                return true;
            }
        }
        #endregion Cyclicality Checker
    }
}
