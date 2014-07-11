using System.Collections.Generic;

namespace Elders.Cronus.DomainModelling
{

    public interface IDomainMessageCommit : IMessage
    {
        IAggregateRootState State { get; }

        List<IEvent> Events { get; }
    }
}