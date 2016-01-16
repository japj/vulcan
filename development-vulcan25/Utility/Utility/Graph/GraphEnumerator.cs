using System;
using System.Collections;
using System.Collections.Generic;
using Vulcan.Utility.Collections;

namespace Vulcan.Utility.Graph
{
    public enum GraphSearchAlgorithm
    {
        BreadthFirstSearch,
        DepthFirstSearch,
        TopographicalSearch,
        ReverseTopographicalSearch,
    }

    public class GraphEnumerator<T> : IEnumerator<T>
    {
        private IOneInOneOutCollection<GraphNode<T>> _cache;

        public Graph<T> Graph { get; private set; }

        public GraphSearchAlgorithm GraphSearchAlgorithm { get; private set; }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (_cache.Count == 0)
            {
                return false;
            }

            Current = _cache.Remove().Item;
            return true;
        }

        public void Reset()
        {
            Current = default(T);
            LoadIteratorCache();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Current = default(T);
                Graph = null;
                _cache = null;
            }
        }

        // TODO: Double check these when you are less tired
        private void LoadIteratorCache()
        {
            var visited = new List<GraphNode<T>>();
            IOneInOneOutCollection<GraphNode<T>> remaining;

            switch (GraphSearchAlgorithm)
            {
                case GraphSearchAlgorithm.BreadthFirstSearch:
                    remaining = new FirstInFirstOutCollection<GraphNode<T>>();
                    _cache = new FirstInFirstOutCollection<GraphNode<T>>();
                    break;
                case GraphSearchAlgorithm.DepthFirstSearch:
                    remaining = new LastInFirstOutCollection<GraphNode<T>>();
                    _cache = new FirstInFirstOutCollection<GraphNode<T>>();
                    break;
                case Utility.Graph.GraphSearchAlgorithm.TopographicalSearch:
                    if (!Graph.IsAcyclic)
                    {
                        throw new NotSupportedException("Cannot invoke a topographical search on a graph with cycles");
                    }

                    remaining = new LastInFirstOutCollection<GraphNode<T>>();
                    _cache = new LastInFirstOutCollection<GraphNode<T>>();
                    break;
                case Utility.Graph.GraphSearchAlgorithm.ReverseTopographicalSearch:
                    if (!Graph.IsAcyclic)
                    {
                        throw new NotSupportedException("Cannot invoke a reverse topographical search on a graph with cycles");
                    }

                    remaining = new LastInFirstOutCollection<GraphNode<T>>();
                    _cache = new FirstInFirstOutCollection<GraphNode<T>>();
                    break;
                default:
                    throw new InvalidOperationException("Unknown graph search algorithm");
            }

            remaining.AddRange(Graph.RootNodes);

            while (remaining.Count > 0)
            {
                GraphNode<T> currentNode = remaining.Remove();
                visited.Add(currentNode);

                foreach (GraphEdge<T> outgoingEdge in currentNode.OutgoingEdges)
                {
                    GraphNode<T> successor = outgoingEdge.Sink;
                    if (!visited.Contains(successor) && !remaining.FastContains(successor))
                    {
                        remaining.Add(successor);
                    }
                }

                _cache.Add(currentNode);
            }
        }

        public GraphEnumerator(Graph<T> graph, GraphSearchAlgorithm graphSearchAlgorithm)
        {
            Graph = graph;
            GraphSearchAlgorithm = graphSearchAlgorithm;

            LoadIteratorCache();
        }
    }
}
