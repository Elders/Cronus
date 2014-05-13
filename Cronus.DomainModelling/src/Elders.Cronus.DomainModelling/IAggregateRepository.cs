using System;

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
        [Obsolete("Use the overload without IAggreagteRootId parameter. The ICommand now holds the aggregate id.")]
        AR Update<AR>(IAggregateRootId aggregateId, ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot;
        AR Update<AR>(ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot;

        void Save(IAggregateRoot aggregateRoot, ICommand command);
    }

}
