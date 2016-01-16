using System;

namespace Vulcan.Utility.Collections
{
    public interface IObservableCollectionSynchronizer
    {
        event EventHandler<EventArgs> FireChangedEvents;
    }
}
