using System;

namespace Elders.Cronus.DomainModelling
{
    public interface ICommand : IMessage
    {
        IAggregateRootId MetaAggregateId { get; }
        long MetaTimestamp { get; }

        Guid MetaCommandId { get; }
        int MetaExpectedAggregateRevision { get; }
    }
}