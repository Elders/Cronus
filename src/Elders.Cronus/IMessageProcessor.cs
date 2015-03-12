using System;
using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus
{
    public interface IMessageProcessor
    {
        IFeedResult Feed(List<TransportMessage> messages);
        IEnumerable<Type> GetRegisteredHandlers();
        IDisposable Subscribe(MessageProcessorSubscription subscription);
    }
}
