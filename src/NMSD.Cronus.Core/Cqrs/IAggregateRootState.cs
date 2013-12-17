using System;
using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootState
    {
        IAggregateRootId Id { get; }
        int Version { get; set; }

        void Apply(IEvent @event);
    }
}