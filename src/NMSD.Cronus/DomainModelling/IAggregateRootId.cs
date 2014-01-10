using System;

namespace NMSD.Cronus.DomainModelling
{
    public interface IAggregateRootId : IEquatable<IAggregateRootId>
    {   
        Guid Id { get; set; }
    }
}