using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Vulcan.Utility.Collections
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Parallels naming of HashSet.")]
    public class SynchronizedObservableHashSet<T> : ObservableHashSet<T>
    {
        public bool HasSyncItems
        {
            get { return _cachedCollectionChangedEvents.Count > 0 || _cachedPropertyChangedNames.Count > 0; }
        }

        private IObservableCollectionSynchronizer _observableCollectionSynchronizer;

        public IObservableCollectionSynchronizer ObservableCollectionSynchronizer
        {
            get { return _observableCollectionSynchronizer; }
            set
            {
                if (_observableCollectionSynchronizer != value)
                {
                    if (_observableCollectionSynchronizer != null)
                    {
                        _observableCollectionSynchronizer.FireChangedEvents -= _observableCollectionSynchronizer_FireChangedEvents;                        
                    }

                    _observableCollectionSynchronizer = value;
                    if (_observableCollectionSynchronizer != null)
                    {
                        _observableCollectionSynchronizer.FireChangedEvents += _observableCollectionSynchronizer_FireChangedEvents;
                    }
                }
            }
        }

        private void _observableCollectionSynchronizer_FireChangedEvents(object sender, EventArgs e)
        {
            OnFireChangedEvents();
        }

        private readonly List<NotifyCollectionChangedEventArgs> _cachedCollectionChangedEvents;

        private readonly List<string> _cachedPropertyChangedNames;

        public SynchronizedObservableHashSet()
        {
            _cachedCollectionChangedEvents = new List<NotifyCollectionChangedEventArgs>();
            _cachedPropertyChangedNames = new List<string>();
        }

        public SynchronizedObservableHashSet(IObservableCollectionSynchronizer observableCollectionSynchronizer) : this()
        {
            ObservableCollectionSynchronizer = observableCollectionSynchronizer;
        }
        
        public SynchronizedObservableHashSet(IEnumerable<T> collection) : this()
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
        
        public SynchronizedObservableHashSet(IObservableCollectionSynchronizer observableCollectionSynchronizer, IEnumerable<T> collection) : this(collection)
        {
            ObservableCollectionSynchronizer = observableCollectionSynchronizer;
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            _cachedCollectionChangedEvents.Add(e);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The event data.</param>
        protected override void OnPropertyChanged(string propertyName)
        {
            _cachedPropertyChangedNames.Add(propertyName);
        }

        protected virtual void OnFireChangedEvents()
        {
            var snapshotPropertyChanges = new List<string>(_cachedPropertyChangedNames);
            _cachedPropertyChangedNames.Clear();
            foreach (var propertyName in snapshotPropertyChanges)
            {
                base.OnPropertyChanged(propertyName);
            }

            var snapshotCollectionChanges = new List<NotifyCollectionChangedEventArgs>(_cachedCollectionChangedEvents);
            _cachedCollectionChangedEvents.Clear();
            foreach (var collectionChange in snapshotCollectionChanges)
            {
                base.OnCollectionChanged(collectionChange);
            }
        }
    }
}
