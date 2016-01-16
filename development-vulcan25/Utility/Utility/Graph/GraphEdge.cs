namespace Vulcan.Utility.Graph
{
    public class GraphEdge<T>
    {
        public GraphNode<T> Source { get; private set; }

        public GraphNode<T> Sink { get; private set; }

        public object SourceData { get; private set; }

        public object SinkData { get; private set; }

        public string Label { get; private set; }

        public GraphEdge(GraphNode<T> source, GraphNode<T> sink, string label, object sourceData, object sinkData)
        {
            Source = source;
            Sink = sink;
            Label = label;
            SourceData = sourceData;
            SinkData = sinkData;
        }
    }
}
