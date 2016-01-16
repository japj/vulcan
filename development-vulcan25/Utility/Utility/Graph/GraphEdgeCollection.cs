using System;
using System.Collections.Generic;
using Vulcan.Utility.Collections;

namespace Vulcan.Utility.Graph
{
    public class GraphEdgeCollection<T> : ObservableHashSet<GraphEdge<T>>
    {
        public Graph<T> Graph { get; private set; }
        
        public GraphEdgeCollection(Graph<T> graph)
        {
            Graph = graph;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public GraphEdgeCollection(Graph<T> graph, IEnumerable<GraphEdge<T>> collection) : this(graph)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            foreach (var item in collection)
            {
                Add(item);
            }
        }
    }
}
