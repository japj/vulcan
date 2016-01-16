using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security;

namespace Vulcan.Utility.Collections
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name follows the naming convention of the Graph library.")]
    public class ObservableHashSet<T> : ICollection<T>
    {
        private ObservableDictionary<T, T> _store;

        public ObservableHashSet()
        {
            _store = new ObservableDictionary<T, T>();
            _store.CollectionChanged += _store_CollectionChanged;
        }

        public ObservableHashSet(IEnumerable<T> collection)
            : this()
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            foreach (var item in collection)
            {
                Add(item);
            }
        }

        ////public HashSet(IEqualityComparer<T> comparer);
        ////public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer);
        ////protected HashSet(SerializationInfo info, StreamingContext context);

        private void _store_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<T> oldItems = null;
            List<T> newItems = null;

            if (e.OldItems != null)
            {
                oldItems = new List<T>();
                foreach (KeyValuePair<T, T> oldPair in e.OldItems)
                {
                    oldItems.Add(oldPair.Key);
                }
            }

            if (e.NewItems != null)
            {
                newItems = new List<T>();
                foreach (KeyValuePair<T, T> newPair in e.NewItems)
                {
                    newItems.Add(newPair.Key);
                }
            }

            NotifyCollectionChangedEventArgs eventArgs;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: 
                    eventArgs = new NotifyCollectionChangedEventArgs(e.Action, newItems, e.NewStartingIndex); 
                    break;
                case NotifyCollectionChangedAction.Move:
                    eventArgs = new NotifyCollectionChangedEventArgs(e.Action, newItems, e.NewStartingIndex, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove: 
                    eventArgs = new NotifyCollectionChangedEventArgs(e.Action, oldItems, e.OldStartingIndex); 
                    break;
                case NotifyCollectionChangedAction.Replace: 
                    eventArgs = new NotifyCollectionChangedEventArgs(e.Action, newItems, oldItems, e.NewStartingIndex); 
                    break;
                case NotifyCollectionChangedAction.Reset: 
                    eventArgs = new NotifyCollectionChangedEventArgs(e.Action); 
                    break;
                default: 
                    throw new ArgumentException("Unknown Collection Action Type");
            }

            // TODO: Likely need to add action specific transformation layer
            OnCollectionChanged(eventArgs);
        }

        /// <summary>
        /// Occurs when an item is added, removed, or changed, or the entire list is refreshed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The event data.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //// <summary>
        ////     Gets the System.Collections.Generic.IEqualityComparer<T> object that is used
        ////     to determine equality for the values in the set.
        //// </summary>
        //// Returns:
        ////     The System.Collections.Generic.IEqualityComparer<T> object that is used to
        ////     determine equality for the values in the set.
        ////public IEqualityComparer<T> Comparer { get; }

        /// <summary>
        ///     Gets the number of elements that are contained in a set.
        /// </summary>
        /// <value>The number of elements that are contained in the set.</value>
        public int Count 
        { 
            get { return _store.Count; } 
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Adds the specified element to a set.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>
        ///     true if the element is added to the System.Collections.Generic.HashSet<T>
        ///     object; false if the element is already present.
        /// </returns>
        public bool Add(T item)
        {
            if (!_store.ContainsKey(item))
            {
                _store.Add(item, item);
                return true;
            }

            return false;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        ///     Removes all elements from a System.Collections.Generic.HashSet<T> object.
        /// </summary>
        public void Clear()
        {
            _store.Clear();
        }

        /// <summary>
        ///     Determines whether a System.Collections.Generic.HashSet<T> object contains
        ///     the specified element.
        /// </summary>
        /// <param name="item">The element to locate in the System.Collections.Generic.HashSet<T> object.</param>
        ///
        /// <returns>
        ///     true if the System.Collections.Generic.HashSet<T> object contains the specified
        ///     element; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return _store.ContainsKey(item);
        }

        /// <summary>
        ///     Copies the elements of a System.Collections.Generic.HashSet<T> object to
        ///     an array.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional array that is the destination of the elements copied
        ///     from the System.Collections.Generic.HashSet<T> object. The array must have
        ///     zero-based indexing.
        /// </param>
        /// <remarks>
        /// Exceptions:
        ///   System.ArgumentNullException: array is null.
        /// </remarks>
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        // Summary:
        //     Copies the elements of a System.Collections.Generic.HashSet<T> object to
        //     an array, starting at the specified array index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional array that is the destination of the elements copied
        //     from the System.Collections.Generic.HashSet<T> object. The array must have
        //     zero-based indexing.
        //
        //   arrayIndex:
        //     The zero-based index in array at which copying begins.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     arrayIndex is less than 0.
        //
        //   System.ArgumentException:
        //     arrayIndex is greater than the length of the destination array.  -or- count
        //     is larger than the size of the destination array.
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, _store.Keys.Count);
        }

        // Summary:
        //     Copies the specified number of elements of a System.Collections.Generic.HashSet<T>
        //     object to an array, starting at the specified array index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional array that is the destination of the elements copied
        //     from the System.Collections.Generic.HashSet<T> object. The array must have
        //     zero-based indexing.
        //
        //   arrayIndex:
        //     The zero-based index in array at which copying begins.
        //
        //   count:
        //     The number of elements to copy to array.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     arrayIndex is less than 0.  -or- count is less than 0.
        //
        //   System.ArgumentException:
        //     arrayIndex is greater than the length of the destination array.  -or- count
        //     is greater than the available space from the index to the end of the destination
        //     array.
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (arrayIndex > array.Length)
            {
                throw new ArgumentException("arrayIndex cannot be greater than the length of the destination array.");
            }

            if (count > array.Length - arrayIndex)
            {
                throw new ArgumentException("count cannot be greater than the available space from the index to the end of the destination array.");
            }

            int counter = 0;
            foreach (var key in _store.Keys)
            {
                if (count == counter)
                {
                    break;
                }

                array[arrayIndex] = key;
                ++arrayIndex;
                ++counter;
            }
        }

        //// Summary:
        ////     Returns an System.Collections.IEqualityComparer object that can be used for
        ////     deep equality testing of a System.Collections.Generic.HashSet<T> object.
        ////
        //// Returns:
        ////     An System.Collections.IEqualityComparer object that can be used for deep
        ////     equality testing of the System.Collections.Generic.HashSet<T> object.
        ////public static IEqualityComparer<HashSet<T>> CreateSetComparer();
        
        // Summary:
        //     Removes all elements in the specified collection from the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Parameters:
        //   other:
        //     The collection of items to remove from the System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var item in other)
            {
                _store.Remove(item);
            }
        }

        // Summary:
        //     Returns an enumerator that iterates through a System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     A System.Collections.Generic.HashSet<T>.Enumerator object for the System.Collections.Generic.HashSet<T>
        //     object.
        public IEnumerator<T> GetEnumerator()
        {
            return _store.Keys.GetEnumerator();
        }

        //// Summary:
        ////     Implements the System.Runtime.Serialization.ISerializable interface and returns
        ////     the data needed to serialize a System.Collections.Generic.HashSet<T> object.
        ////
        //// Parameters:
        ////   info:
        ////     A System.Runtime.Serialization.SerializationInfo object that contains the
        ////     information required to serialize the System.Collections.Generic.HashSet<T>
        ////     object.
        ////
        ////   context:
        ////     A System.Runtime.Serialization.StreamingContext structure that contains the
        ////     source and destination of the serialized stream associated with the System.Collections.Generic.HashSet<T>
        ////     object.
        ////
        //// Exceptions:
        ////   System.ArgumentNullException:
        ////     info is null.
        ////public virtual void GetObjectData(SerializationInfo info, StreamingContext context);

        // Summary:
        //     Modifies the current System.Collections.Generic.HashSet<T> object to contain
        //     only elements that are present in that object and in the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        [SecurityCritical]
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            IntersectWith(new ObservableHashSet<T>(other));
        }

        [SecurityCritical]
        public void IntersectWith(ObservableHashSet<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var key in _store.Keys)
            {
                if (!other.Contains(key))
                {
                    _store.Remove(key);
                }
            }
        }

        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object is a proper
        //     subset of the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is a proper subset
        //     of other; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        [SecurityCritical]
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return IsProperSubsetOf(new ObservableHashSet<T>(other));
        }

        [SecurityCritical]
        public bool IsProperSubsetOf(ObservableHashSet<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var item in _store.Keys)
            {
                if (!other.Contains(item))
                {
                    return false;
                }
            }

            return other.Count > Count;
        }

        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object is a proper
        //     superset of the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is a proper superset
        //     of other; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        [SecurityCritical]
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            int otherCount = 0;
            foreach (var item in other)
            {
                if (!_store.ContainsKey(item))
                {
                    return false;
                }

                ++otherCount;
            }

            return Count > otherCount;
        }

        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object is a subset
        //     of the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is a subset of other;
        //     otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        [SecurityCritical]
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return IsSubsetOf(new ObservableHashSet<T>(other));
        }

        [SecurityCritical]
        public bool IsSubsetOf(ObservableHashSet<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var item in _store.Keys)
            {
                if (!other.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object is a superset
        //     of the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is a superset of
        //     other; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var item in other)
            {
                if (!_store.ContainsKey(item))
                {
                    return false;
                }
            }

            return true;
        }

        //// Summary:
        ////     Implements the System.Runtime.Serialization.ISerializable interface and raises
        ////     the deserialization event when the deserialization is complete.
        ////
        //// Parameters:
        ////   sender:
        ////     The source of the deserialization event.
        ////
        //// Exceptions:
        ////   System.Runtime.Serialization.SerializationException:
        ////     The System.Runtime.Serialization.SerializationInfo object associated with
        ////     the current System.Collections.Generic.HashSet<T> object is invalid.
        ////public virtual void OnDeserialization(object sender);

        // Summary:
        //     Determines whether the current System.Collections.Generic.HashSet<T> object
        //     overlaps the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object and other share
        //     at least one common element; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var item in other)
            {
                if (_store.ContainsKey(item))
                {
                    return true;
                }
            }

            return false;
        }

        // Summary:
        //     Removes the specified element from a System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Parameters:
        //   item:
        //     The element to remove.
        //
        // Returns:
        //     true if the element is successfully found and removed; otherwise, false.
        //     This method returns false if item is not found in the System.Collections.Generic.HashSet<T>
        //     object.
        public bool Remove(T item)
        {
            if (item == null)
            {
                return false;
            }

            return _store.Remove(item);
        }

        // Summary:
        //     Removes all elements that match the conditions defined by the specified predicate
        //     from a System.Collections.Generic.HashSet<T> collection.
        //
        // Parameters:
        //   match:
        //     The System.Predicate<T> delegate that defines the conditions of the elements
        //     to remove.
        //
        // Returns:
        //     The number of elements that were removed from the System.Collections.Generic.HashSet<T>
        //     collection.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     match is null.
        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            int count = 0;
            foreach (var key in _store.Keys)
            {
                if (match(key))
                {
                    _store.Remove(key);
                    ++count;
                }
            }

            return count;
        }

        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object and the
        //     specified collection contain the same elements.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is equal to other;
        //     otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        [SecurityCritical]
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            int otherCount = 0;
            foreach (var item in other)
            {
                if (!_store.ContainsKey(item))
                {
                    return false;
                }

                ++otherCount;
            }

            return Count == otherCount;
        }

        // Summary:
        //     Modifies the current System.Collections.Generic.HashSet<T> object to contain
        //     only elements that are present either in that object or in the specified
        //     collection, but not both.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        [SecurityCritical]
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var item in other)
            {
                if (_store.ContainsKey(item))
                {
                    _store.Remove(item);
                }
                else
                {
                    _store.Add(item, item);
                }
            }
        }

        // Summary:
        //     Sets the capacity of a System.Collections.Generic.HashSet<T> object to the
        //     actual number of elements it contains, rounded up to a nearby, implementation-specific
        //     value.
        public void TrimExcess()
        {
            // no-op
        }

        // Summary:
        //     Modifies the current System.Collections.Generic.HashSet<T> object to contain
        //     all elements that are present in both itself and in the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var item in other)
            {
                Add(item);
            }
        }

        /*
        // Summary:
        //     Enumerates the elements of a System.Collections.Generic.HashSet<T> object.
        [Serializable]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {

            // Summary:
            //     Gets the element at the current position of the enumerator.
            //
            // Returns:
            //     The element in the System.Collections.Generic.HashSet<T> collection at the
            //     current position of the enumerator.
            public T Current { get; }

            // Summary:
            //     Releases all resources used by a System.Collections.Generic.HashSet<T>.Enumerator
            //     object.
            public void Dispose();
            //
            // Summary:
            //     Advances the enumerator to the next element of the System.Collections.Generic.HashSet<T>
            //     collection.
            //
            // Returns:
            //     true if the enumerator was successfully advanced to the next element; false
            //     if the enumerator has passed the end of the collection.
            //
            // Exceptions:
            //   System.InvalidOperationException:
            //     The collection was modified after the enumerator was created.
            public bool MoveNext();
        }
        */
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
