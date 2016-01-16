namespace Vulcan.Utility.Graph
{
    public interface IGraphNodeBuilder<T>
    {
        GraphNode<T> AddNode(Graph<T> graph, T item);
    }
}
