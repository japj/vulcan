namespace Vulcan.Utility.Graph
{
    public interface IGraphEdgeRelocator<T>
    {
        void RelocateEdge(Graph<T> graph, GraphEdge<T> edge, GraphNode<T> newSourceNode, GraphNode<T> newSinkNode);
    }
}
