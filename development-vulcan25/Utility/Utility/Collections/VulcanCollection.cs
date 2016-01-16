using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Vulcan.Utility.Xml;

namespace Vulcan.Utility.Collections
{
    /// <summary>
    /// Vulcan wrapper for ObservableCollection. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// The wrapper class design makes it easy to change the underlying type.
    /// The class extends from ObservableCollection so bound controls are notified
    /// when items are added or removed.
    /// </remarks>
    [DataContract]
    public class VulcanCollection<T> : ObservableCollection<T>, IXObjectMappingProvider
    {
        public XObjectMapping BoundXObject { get; set; }

        protected override void ClearItems()
        {
            while (Count > 0)
            {
                RemoveAt(0);
            }

            base.ClearItems();
        }

        public VulcanCollection() : base()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Parallels constructor exposed by ObservableCollection.")]
        public VulcanCollection(List<T> list) : base(list)
        {
        }

        public VulcanCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        public void Replace(T item, T replacement)
        {
            int index = IndexOf(item);
            RemoveAt(index);
            Insert(index, replacement);
        }

        public void Replace(T item, IEnumerable<T> replacements)
        {
            int index = IndexOf(item);
            RemoveAt(index);
            foreach (T replacement in replacements.Reverse())
            {
                Insert(index, replacement);
            }
        }
    }
}
