using System;
using System.Collections.Generic;

namespace Vulcan.Utility.Tree
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This class represents a tree node and not a collection.")]
    public class TreeNode<T> : IEnumerable<T> where T : TreeNode<T>
    {
        private T parentNode;

        public ICollection<T> Children { get; private set; }

        public int Count
        {
            get
            {
                int count = 0;
                var enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    count++;
                }

                return count;
            }
        }

        public int Depth 
        { 
            get
            {
                if (Parent == null)
                {
                    return 0;
                }

                return Parent.Depth + 1;
            }
        }

        public T Root
        {
            get
            {
                if (Parent == null)
                {
                    // TODO: How can we cast TreeNode<T> to T?
                    return (T)this;
                }

                return Parent.Root;
            }
        }

        // TODO: How can we cast TreeNode<T> to T?
        public T Parent
        {
            get { return parentNode; }
            set
            {
                if (value == Parent)
                {
                    return;
                }

                if (parentNode != null)
                {
                    var oldParent = parentNode;
                    parentNode = null;
                    oldParent.Children.Remove((T)this);
                }

                if (value != null)
                {
                    if (!value.Children.Contains((T)this))
                    {
                        value.Children.Add((T)this);
                    }
                }

                parentNode = value;
            }
        }

        public TreeNode()
        {
            parentNode = null;
            Children = new TreeNodeList<T>((T)this);
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumeratorDepthFirstSearch();
        }

        private IEnumerator<T> GetEnumeratorDepthFirstSearch()
        {
            yield return (T)this;
            foreach (var child in Children)
            {
                var childEnumerator = child.GetEnumeratorDepthFirstSearch();
                while (childEnumerator.MoveNext())
                {
                    yield return childEnumerator.Current;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
