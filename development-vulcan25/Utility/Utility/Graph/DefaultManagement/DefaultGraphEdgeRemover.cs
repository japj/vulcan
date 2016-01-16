namespace Vulcan.Utility.Graph.DefaultManagement
{
    public class DefaultGraphEdgeRemover<T> : IGraphEdgeRemover<T>
    {
        public bool RemoveEdge(Graph<T> graph, GraphEdge<T> edge)
        {
            if (edge == null)
            {
                return false;
            }

            return graph.Edges.Remove(edge);
        }
    }
}