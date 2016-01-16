namespace Vulcan.Utility.Graph.DefaultManagement
{
    public class DefaultGraphEdgeBuilder<T> : IGraphEdgeBuilder<T>
    {
        public GraphEdge<T> AddEdge(Graph<T> graph, GraphEdge<T> edge)
        {
            graph.Edges.Add(edge);
            return edge;
        }
    }
}