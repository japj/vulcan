using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Utility.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name follows the naming convention of the Graph library.")]
    public class GraphEnumerable<TFollow, TReturn> : IEnumerable<TReturn> where TReturn : TFollow
    {
        public TFollow Root { get; private set; }

        public GraphSearchAlgorithm GraphSearchAlgorithm { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public Func<TFollow, IEnumerable<TFollow>> ChildrenProjector { get; private set; }

        public IEnumerator<TReturn> GetEnumerator()
        {
            return new GraphEnumerator<TFollow, TReturn>(Root, GraphSearchAlgorithm, ChildrenProjector);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public GraphEnumerable(TFollow root) : this(root, GraphSearchAlgorithm.DepthFirstSearch, (item => CommonUtility.AllPropertyChildren<TFollow>(item)))
        {
        }

        public GraphEnumerable(TFollow root, GraphSearchAlgorithm graphSearchAlgorithm) : this(root, graphSearchAlgorithm, (item => CommonUtility.AllPropertyChildren<TFollow>(item)))
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Graph APIs are special purpose routines used by advanced developers.")]
        public GraphEnumerable(TFollow root, GraphSearchAlgorithm graphSearchAlgorithm, Func<TFollow, IEnumerable<TFollow>> childrenProjector)
        {
            Root = root;
            GraphSearchAlgorithm = graphSearchAlgorithm;
            ChildrenProjector = childrenProjector;
        }
    }
}
