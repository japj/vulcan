using System;
using System.Collections.Generic;
using Vulcan.Utility.Collections;

namespace Vulcan.Utility.Graph
{
    public class GraphNodeCollection<T> : ObservableHashSet<GraphNode<T>>
    {
        public Graph<T> Graph { get; private set; }
        
        public GraphNodeCollection(Graph<T> graph)
        {
            Graph = graph;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public GraphNodeCollection(Graph<T> graph, IEnumerable<GraphNode<T>> collection) : this(graph)
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
