namespace Vulcan.Utility.Graph.DefaultManagement
{
    public class DefaultGraphManager<T> : IGraphManager<T>
    {
        public IGraphNodeBuilder<T> GraphNodeBuilder { get; private set; }

        public IGraphNodeRemover<T> GraphNodeRemover { get; private set; }

        public IGraphEdgeBuilder<T> GraphEdgeBuilder { get; private set; }

        public IGraphEdgeRemover<T> GraphEdgeRemover { get; private set; }

        public IGraphEdgeRelocator<T> GraphEdgeRelocator { get; private set; }

        public DefaultGraphManager()
        {
            GraphNodeBuilder = new DefaultGraphNodeBuilder<T>();
            GraphNodeRemover = new DefaultGraphNodeRemover<T>();
            
            GraphEdgeBuilder = new DefaultGraphEdgeBuilder<T>();
            GraphEdgeRemover = new DefaultGraphEdgeRemover<T>();
            GraphEdgeRelocator = new DefaultGraphEdgeRelocator<T>();
        }
    }
}