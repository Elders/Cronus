using System;
using System.Collections.Generic;

namespace Elders.Cronus.DomainModelling
{
    /// <summary>
    /// Indicates the ability to store and retreive a stream of events.
    /// </summary>
    /// <remarks>
    /// Instances of this class must be designed to be multi-thread safe such that they can be shared between threads.
    /// </remarks>
    public interface IAggregateRepository
    {
        void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot;

        AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot;
    }

    public interface IApplicationServiceGateway
    {
        void CommitChanges(Action<IEvent> publish);
    }

}
