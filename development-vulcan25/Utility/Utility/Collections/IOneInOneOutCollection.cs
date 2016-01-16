using System.Collections.Generic;

namespace Vulcan.Utility.Collections
{
    internal interface IOneInOneOutCollection<T>
    {
        void Add(T item);

        void AddRange(IEnumerable<T> item);

        T Remove();

        T Peek();

        void Clear();

        bool FastContains(T item);

        int Count { get; }
    }
}
