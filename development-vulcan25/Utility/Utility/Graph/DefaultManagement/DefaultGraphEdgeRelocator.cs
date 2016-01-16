namespace Vulcan.Utility.Graph.DefaultManagement
{
    public class DefaultGraphEdgeRelocator<T> : IGraphEdgeRelocator<T>
    {
        public void RelocateEdge(Graph<T> graph, GraphEdge<T> edge, GraphNode<T> newSourceNode, GraphNode<T> newSinkNode)
        {
            graph.RemoveEdge(edge);
            graph.AddEdge(newSourceNode, newSinkNode);
        }
    }
}