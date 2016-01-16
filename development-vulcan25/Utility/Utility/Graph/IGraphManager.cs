namespace Vulcan.Utility.Graph
{
    public interface IGraphManager<T>
    {
        IGraphNodeBuilder<T> GraphNodeBuilder { get; }

        IGraphNodeRemover<T> GraphNodeRemover { get; }

        IGraphEdgeBuilder<T> GraphEdgeBuilder { get; }

        IGraphEdgeRemover<T> GraphEdgeRemover { get; }

        IGraphEdgeRelocator<T> GraphEdgeRelocator { get; }
    }
}
