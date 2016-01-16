using System.Collections;
using System.Collections.Specialized;

namespace Vulcan.Utility.Collections
{
    public class VulcanCollectionPropertyChangedEventArgs : NotifyCollectionChangedEventArgs
    {
        public string PropertyName { get; set; }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action)
            : base(action)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, IList changedItems)
            : base(action, changedItems)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, object changedItem)
            : base(action, changedItem)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, IList newItems, IList oldItems)
            : base(action, newItems, oldItems)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
            : base(action, changedItems, startingIndex)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, object changedItem, int index)
            : base(action, changedItem, index)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, object newItem, object oldItem)
            : base(action, newItem, oldItem)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startIndex)
            : base(action, newItems, oldItems, startIndex)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
            : base(action, changedItems, index, oldIndex)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
            : base(action, changedItem, index, oldIndex)
        {
            PropertyName = propertyName;
        }

        public VulcanCollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
            : base(action, newItem, oldItem, index)
        {
            PropertyName = propertyName;
        }
    }
}
