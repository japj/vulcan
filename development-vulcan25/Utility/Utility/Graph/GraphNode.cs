using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vulcan.Utility.Collections;
using Vulcan.Utility.ComponentModel;

namespace Vulcan.Utility.Graph
{
    public class GraphNode<T> : VulcanNotifyPropertyChanged
    {
        private readonly ObservableHashSet<GraphNode<T>> _dominates;
        private readonly ObservableHashSet<GraphNode<T>> _dominatedBy;
        private readonly ObservableHashSet<GraphNode<T>> _immediatelyDominates;
        private GraphNode<T> _immediatelyDominatedBy;
        private readonly ObservableHashSet<GraphNode<T>> _dominanceFrontier;
        private readonly ObservableHashSet<GraphNode<T>> _dominanceFrontierOf;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public VulcanCollection<GraphEdge<T>> IncomingEdges { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public VulcanCollection<GraphEdge<T>> OutgoingEdges { get; private set; }

        public T Item { get; private set; }

        public bool IsRootNode
        {
            get { return IncomingEdges.Count == 0; }
        }

        public bool IsLeafNode
        {
            get { return OutgoingEdges.Count == 0; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public IEnumerable<GraphNode<T>> ImmediatePredecessors
        {
            get
            {
                var immediatePredecessors = new Collection<GraphNode<T>>();
                foreach (var incomingEdge in IncomingEdges)
                {
                    immediatePredecessors.Add(incomingEdge.Source);
                }

                return immediatePredecessors;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public IEnumerable<GraphNode<T>> ImmediateSuccessors
        {
            get
            {
                var immediateSuccessors = new Collection<GraphNode<T>>();
                foreach (var outgoingEdge in OutgoingEdges)
                {
                    immediateSuccessors.Add(outgoingEdge.Sink);
                }

                return immediateSuccessors;
            }
        }

        public Graph<T> Graph { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public ObservableHashSet<GraphNode<T>> Dominates 
        { 
            get 
            { 
                Graph.EnsureUpdated();
                return _dominates;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public ObservableHashSet<GraphNode<T>> DominatedBy
        {
            get
            {
                Graph.EnsureUpdated();
                return _dominatedBy;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public ObservableHashSet<GraphNode<T>> ImmediatelyDominates
        {
            get
            {
                Graph.EnsureUpdated();
                return _immediatelyDominates;
            }
        }

        public GraphNode<T> ImmediatelyDominatedBy
        {
            get
            {
                Graph.EnsureUpdated();
                return _immediatelyDominatedBy;
            }

            set
            {
                if (_immediatelyDominatedBy != value)
                {
                    var oldValue = _immediatelyDominatedBy;
                    _immediatelyDominatedBy = value;
                    VulcanOnPropertyChanged("ImmediatelyDominatedBy", oldValue, _immediatelyDominatedBy);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public ObservableHashSet<GraphNode<T>> DominanceFrontier
        {
            get
            {
                Graph.EnsureUpdated();
                return _dominanceFrontier;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public ObservableHashSet<GraphNode<T>> DominanceFrontierOf
        {
            get
            {
                Graph.EnsureUpdated();
                return _dominanceFrontierOf;
            }
        }

        public bool IsLeader
        {
            get { return IncomingEdges.Count != 1 || IncomingEdges[0].Source.OutgoingEdges.Count > 1; }
        }

        public GraphNode(Graph<T> graph, T item)
        {
            Graph = graph;
            Item = item;
            IncomingEdges = new VulcanCollection<GraphEdge<T>>();
            OutgoingEdges = new VulcanCollection<GraphEdge<T>>();

            _dominates = new ObservableHashSet<GraphNode<T>>() { this };
            _dominatedBy = new ObservableHashSet<GraphNode<T>>() { this };
            _immediatelyDominates = new ObservableHashSet<GraphNode<T>>();
            _dominanceFrontier = new ObservableHashSet<GraphNode<T>>();
            _dominanceFrontierOf = new ObservableHashSet<GraphNode<T>>();
        }
    }
}
