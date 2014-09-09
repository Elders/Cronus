using System;

namespace Elders.Cronus.DomainModeling
{
    public interface IAggregateRootId : IEquatable<IAggregateRootId>
    {   
        Guid Id { get; set; }
    }
}