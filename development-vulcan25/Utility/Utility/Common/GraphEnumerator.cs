using System;
using System.Collections;
using System.Collections.Generic;
using Vulcan.Utility.Collections;

namespace Vulcan.Utility.Common
{
    public enum GraphSearchAlgorithm
    {
        BreadthFirstSearch,
        DepthFirstSearch,
    }

    public class GraphEnumerator<TFollow, TReturn> : IEnumerator<TReturn> where TReturn : TFollow
    {
        private HashSet<TFollow> _visited;

        private IOneInOneOutCollection<TFollow> _remaining;

        public TFollow Root { get; private set; }

        public GraphSearchAlgorithm GraphSearchAlgorithm { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public Func<TFollow, IEnumerable<TFollow>> ChildrenProjector { get; private set; }

        public TReturn Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            while (_remaining.Count > 0)
            {
                TFollow currentObject = _remaining.Remove();
                _visited.Add(currentObject);

                foreach (TFollow child in ChildrenProjector(currentObject))
                {
                    if (!_visited.Contains(child) && !_remaining.FastContains(child))
                    {
                        _remaining.Add(child);
                    }
                }

                if (currentObject is TReturn)
                {
                    Current = (TReturn)currentObject;
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            Current = default(TReturn);
            _visited.Clear();
            _remaining.Clear();
            _remaining.Add(Root);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Current = default(TReturn);
                Root = default(TFollow);
                _visited = null;
                _remaining = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public GraphEnumerator(TFollow root, GraphSearchAlgorithm graphSearchAlgorithm, Func<TFollow, IEnumerable<TFollow>> childrenProjector)
        {
            Root = root;
            GraphSearchAlgorithm = graphSearchAlgorithm;
            ChildrenProjector = childrenProjector;
            
            _visited = new HashSet<TFollow>();
            switch (GraphSearchAlgorithm)
            {
                case GraphSearchAlgorithm.BreadthFirstSearch:
                    _remaining = new FirstInFirstOutCollection<TFollow>();
                    break;
                case GraphSearchAlgorithm.DepthFirstSearch:
                    _remaining = new LastInFirstOutCollection<TFollow>();
                    break;
                default:
                    throw new ArgumentException("Unknown graph search algorithm", "graphSearchAlgorithm");
            }

            _remaining.Add(Root);
        }
    }
}
