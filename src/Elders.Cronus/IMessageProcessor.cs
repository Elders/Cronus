using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus
{
    public interface IMessageProcessor<TMessage> where TMessage : IMessage
    {
        ISafeBatchResult<TransportMessage> Handle(List<TransportMessage> messages);
        IEnumerable<Type> GetRegisteredHandlers();
        IDisposable Subscribe(Subscription subscription);
    }
}