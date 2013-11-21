using System;
using Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootState
    {
        IAggregateRootId Id { get; }
        void Apply(IEvent @event);
    }
}