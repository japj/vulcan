using System.Collections;
using System.Collections.Generic;

namespace Vulcan.Utility.Graph
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name follows the naming convention of the Graph library.")]
    public class GraphEnumerable<T> : IEnumerable<T>
    {
        public Graph<T> Graph { get; private set; }

        public GraphSearchAlgorithm GraphSearchAlgorithm { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            return new GraphEnumerator<T>(Graph, GraphSearchAlgorithm);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public GraphEnumerable(Graph<T> graph) : this(graph, GraphSearchAlgorithm.DepthFirstSearch)
        {
        }

        public GraphEnumerable(Graph<T> graph, GraphSearchAlgorithm graphSearchAlgorithm)
        {
            Graph = graph;
            GraphSearchAlgorithm = graphSearchAlgorithm;
        }
    }
}
