namespace Vulcan.Utility.Graph
{
    public interface IGraphEdgeRemover<T>
    {
        bool RemoveEdge(Graph<T> graph, GraphEdge<T> edge);
    }
}
