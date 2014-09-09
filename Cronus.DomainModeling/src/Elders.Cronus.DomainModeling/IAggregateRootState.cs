using System;
using System.Collections.Generic;

namespace Elders.Cronus.DomainModeling
{
    public interface IAggregateRootState : IEqualityComparer<IAggregateRootState>, IEquatable<IAggregateRootState>
    {
        IAggregateRootId Id { get; }
        int Version { get; set; }

        void Apply(IEvent @event);
    }
}