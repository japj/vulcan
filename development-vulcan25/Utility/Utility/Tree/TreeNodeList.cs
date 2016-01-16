using System.Collections;
using System.Collections.Generic;

namespace Vulcan.Utility.Tree
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This class represents a tree node list and not a collection.")]
    public sealed class TreeNodeList<T> : ICollection<T> where T : TreeNode<T>
    {
        private List<T> nodeList;

        public T Parent { get; set; }

        public TreeNodeList(T parent)
        {
            Parent = parent;
            nodeList = new List<T>();
        }

        #region ICollection<TreeNode<T>> Members

        void ICollection<T>.Add(T item)
        {
            if (item != null)
            {
                nodeList.Add(item);
                item.Parent = Parent;
            }
        }

        void ICollection<T>.Clear()
        {
            var nodeRemovalList = new List<T>(nodeList);
            foreach (var node in nodeRemovalList)
            {
                node.Parent = null;
            }
        }

        bool ICollection<T>.Contains(T item)
        {
            return nodeList.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            nodeList.CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count
        {
            get { return nodeList.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<T>.Remove(T item)
        {
            if (item != null)
            {
                item.Parent = null;
                return nodeList.Remove(item);
            }
            
            return false;
        }

        #endregion

        #region IEnumerable<TreeNode<T>> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return nodeList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return nodeList.GetEnumerator();
        }

        #endregion
    }
}
