using System;
using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus
{
    public interface IMessageProcessor
    {
        string Name { get; }
        IFeedResult Feed(List<TransportMessage> messages);
        IEnumerable<MessageProcessorSubscription> GetSubscriptions();
        IDisposable Subscribe(MessageProcessorSubscription subscription);
    }
}
