using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Vulcan.Utility.Collections
{
    /*
    public class SynchronizedObservableCollection<T> : ObservableHashSet<T>
    {
        private IObservableCollectionSynchronizer _observableCollectionSynchronizer;
        public IObservableCollectionSynchronizer ObservableCollectionSynchronizer
        {
            get { return _observableCollectionSynchronizer;  }
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

        void _observableCollectionSynchronizer_FireChangedEvents(object sender, EventArgs e)
        {
            OnFireChangedEvents();
        }

        private readonly List<NotifyCollectionChangedEventArgs> _cachedCollectionChangedEvents;
        private readonly List<string> _cachedPropertyChangedNames;
        public SynchronizedObservableCollection()
        {
            _cachedCollectionChangedEvents = new List<NotifyCollectionChangedEventArgs>();
            _cachedPropertyChangedNames = new List<string>();
        }

        public SynchronizedObservableCollection(IEnumerable<T> collection)
            : this()
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach(var item in collection)
            {
                Add(item);
            }
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
            foreach (var propertyName in _cachedPropertyChangedNames)
            {
                base.OnPropertyChanged(propertyName);
            }
            _cachedPropertyChangedNames.Clear();

            foreach (var collectionChange in _cachedCollectionChangedEvents)
            {
                base.OnCollectionChanged(collectionChange);
            }
            _cachedCollectionChangedEvents.Clear();
        }
    }
    */
}
