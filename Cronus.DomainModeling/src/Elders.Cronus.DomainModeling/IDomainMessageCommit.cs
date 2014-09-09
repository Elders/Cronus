using System.Collections.Generic;

namespace Elders.Cronus.DomainModeling
{

    public interface IDomainMessageCommit : IMessage
    {
        IAggregateRootState State { get; }

        List<IEvent> Events { get; }
    }
}