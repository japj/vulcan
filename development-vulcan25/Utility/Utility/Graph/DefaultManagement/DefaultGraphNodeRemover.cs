namespace Vulcan.Utility.Graph.DefaultManagement
{
    public class DefaultGraphNodeRemover<T> : IGraphNodeRemover<T>
    {
        public bool RemoveNode(Graph<T> graph, GraphNode<T> node)
        {
            if (node == null)
            {
                return false;
            }

            return graph.Nodes.Remove(node);
        }
    }
}