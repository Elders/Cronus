using System;
using System.Collections.Generic;
using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootState : IEqualityComparer<IAggregateRootState>, IEquatable<IAggregateRootState>
    {
        IAggregateRootId Id { get; }
        int Version { get; set; }

        void Apply(IEvent @event);
    }
}