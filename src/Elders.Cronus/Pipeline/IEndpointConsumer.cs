using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointConsumer : ITransportIMessage
    {
        bool Consume(IEndpoint endpoint);
        IEnumerable<Type> GetRegisteredHandlers { get; }
    }

    public interface IEndpointConsumer<out T> : IEndpointConsumer where T : IMessage
    {

    }
}