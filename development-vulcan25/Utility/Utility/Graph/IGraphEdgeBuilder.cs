namespace Vulcan.Utility.Graph
{
    public interface IGraphEdgeBuilder<T>
    {
        GraphEdge<T> AddEdge(Graph<T> graph, GraphEdge<T> edge);
    }
}
