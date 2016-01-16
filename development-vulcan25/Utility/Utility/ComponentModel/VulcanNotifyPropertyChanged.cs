using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using Vulcan.Utility.Collections;

namespace Vulcan.Utility.ComponentModel
{
    [DataContract(IsReference = true)]
    public class VulcanNotifyPropertyChanged : INotifyPropertyChanged
    {
        public static event EventHandler<VulcanPropertyChangedEventArgs> AnyAllPropertyChanged;

        public static event EventHandler<VulcanPropertyChangedEventArgs> AnySingletonPropertyChanged;

        public static event EventHandler<VulcanCollectionPropertyChangedEventArgs> AnyCollectionPropertyChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<VulcanPropertyChangedEventArgs> SingletonPropertyChanged;

        public event EventHandler<VulcanCollectionPropertyChangedEventArgs> CollectionPropertyChanged;

        protected void VulcanOnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (SingletonPropertyChanged != null)
            {
                SingletonPropertyChanged(this, new VulcanPropertyChangedEventArgs(propertyName, oldValue, newValue));
            }

            if (AnySingletonPropertyChanged != null)
            {
                AnySingletonPropertyChanged(this, new VulcanPropertyChangedEventArgs(propertyName, oldValue, newValue));
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new VulcanPropertyChangedEventArgs(propertyName, oldValue, newValue));
            }

            if (AnyAllPropertyChanged != null)
            {
                AnyAllPropertyChanged(this, new VulcanPropertyChangedEventArgs(propertyName, oldValue, newValue));
            }
        }

        protected void VulcanOnCollectionPropertyChanged(string propertyName, NotifyCollectionChangedEventArgs e)
        {
            VulcanCollectionPropertyChangedEventArgs collectionArgs = null;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                collectionArgs = new VulcanCollectionPropertyChangedEventArgs(propertyName, e.Action, e.NewItems, e.NewStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                collectionArgs = new VulcanCollectionPropertyChangedEventArgs(propertyName, e.Action, e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                collectionArgs = new VulcanCollectionPropertyChangedEventArgs(propertyName, e.Action);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                collectionArgs = new VulcanCollectionPropertyChangedEventArgs(propertyName, e.Action, e.OldItems, e.NewStartingIndex, e.OldStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                collectionArgs = new VulcanCollectionPropertyChangedEventArgs(propertyName, e.Action, e.NewItems, e.OldItems, e.NewStartingIndex);
            }

            if (CollectionPropertyChanged != null)
            {
                CollectionPropertyChanged(this, collectionArgs);
            }

            if (AnyCollectionPropertyChanged != null)
            {
                AnyCollectionPropertyChanged(this, collectionArgs);
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new VulcanPropertyChangedEventArgs(propertyName, e.OldItems, e.NewItems));
            }

            if (AnyAllPropertyChanged != null)
            {
                AnyAllPropertyChanged(this, new VulcanPropertyChangedEventArgs(propertyName, e.OldItems, e.NewItems));
            }
        }

        protected static void VulcanCompositeCollectionChanged<T>(VulcanCollection<T> compositeCollection, T oldItem, T newItem)
        {
            compositeCollection.Remove(oldItem);

            if (newItem != null)
            {
                compositeCollection.Add(newItem);
            }
        }

        protected static void VulcanCompositeCollectionChanged<T>(VulcanCollection<T> compositeCollection, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    compositeCollection.Add((T)item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    compositeCollection.Remove((T)item);
                }
            }
        }
    }
}
