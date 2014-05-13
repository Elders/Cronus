using System;

namespace Elders.Cronus.DomainModelling
{
    public interface IAggregateRootId : IEquatable<IAggregateRootId>
    {   
        Guid Id { get; set; }
    }
}