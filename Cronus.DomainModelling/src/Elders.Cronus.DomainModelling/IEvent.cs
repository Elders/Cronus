using System;

namespace Elders.Cronus.DomainModelling
{
    /// <summary>
    /// A markup interface telling that the implementing class is an event.
    /// </summary>
    public interface IEvent : IMessage
    {
        //IAggregateRootId MetaAggregateId { get; }
        //long MetaTimestamp { get; }
    }
}