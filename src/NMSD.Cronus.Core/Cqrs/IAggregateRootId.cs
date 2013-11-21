using System;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootId : IEquatable<IAggregateRootId>
    {
        Guid Id { get; set; }
    }
}