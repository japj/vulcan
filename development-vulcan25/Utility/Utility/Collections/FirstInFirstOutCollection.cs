using System.Collections.Generic;

namespace Vulcan.Utility.Collections
{
    internal class FirstInFirstOutCollection<T> : Queue<T>, IOneInOneOutCollection<T>
    {
        private HashSet<T> _lookupCache;

        public FirstInFirstOutCollection()
        {
            _lookupCache = new HashSet<T>();
        }

        public void Add(T item)
        {
            Enqueue(item);
            _lookupCache.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        public bool FastContains(T item)
        {
            return _lookupCache.Contains(item);
        }

        public T Remove()
        {
            T temp = Dequeue();
            _lookupCache.Remove(temp);
            return temp;
        }
    }
}
