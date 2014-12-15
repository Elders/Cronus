using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus
{
    public interface IMessageProcessor<TMessage> where TMessage : IMessage
    {
        IFeedResult Feed(List<TransportMessage> messages);
        IEnumerable<Type> GetRegisteredHandlers();
        IDisposable Subscribe(MessageProcessorSubscription subscription);
    }
}