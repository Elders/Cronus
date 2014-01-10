using System;

namespace NMSD.Cronus.Core.DomainModelling
{
    public interface IAggregateRootId : IEquatable<IAggregateRootId>
    {   
        Guid Id { get; set; }
    }
}