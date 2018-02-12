using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Pipeline;
using Elders.Cronus.Serializer;

namespace Elders.Cronus
{
    public interface ITransport
    {
        IEnumerable<IConsumerFactory> GetAvailableConsumers(ISerializer serializer, SubscriptionMiddleware subscriptions, string consumerName);
        IPublisher<TMessage> GetPublisher<TMessage>(ISerializer serializer) where TMessage : IMessage;
    }


}
