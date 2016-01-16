namespace Vulcan.Utility.Graph
{
    public interface IGraphNodeRemover<T>
    {
        bool RemoveNode(Graph<T> graph, GraphNode<T> node);
    }
}
