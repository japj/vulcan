namespace Vulcan.Utility.Graph.DefaultManagement
{
    public class DefaultGraphNodeBuilder<T> : IGraphNodeBuilder<T>
    {
        public GraphNode<T> AddNode(Graph<T> graph, T item)
        {
            var node = new GraphNode<T>(graph, item);
            graph.Nodes.Add(node);
            return node;
        }
    }
}