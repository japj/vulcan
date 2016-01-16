using System.Collections.Generic;

namespace Vulcan.Utility.Collections
{
    internal class LastInFirstOutCollection<T> : Stack<T>, IOneInOneOutCollection<T>
    {
        private HashSet<T> _lookupCache;

        public LastInFirstOutCollection()
        {
            _lookupCache = new HashSet<T>();
        }

        public void Add(T item)
        {
            Push(item);
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
            T temp = Pop();
            _lookupCache.Remove(temp);
            return temp;
        }
    }
}
