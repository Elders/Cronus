using System;
using System.Collections.Generic;
using NMSD.Cronus.Eventing;

namespace NMSD.Cronus.DomainModelling
{
    public interface IAggregateRootState : IEqualityComparer<IAggregateRootState>, IEquatable<IAggregateRootState>
    {
        IAggregateRootId Id { get; }
        int Version { get; set; }

        void Apply(IEvent @event);
    }
}