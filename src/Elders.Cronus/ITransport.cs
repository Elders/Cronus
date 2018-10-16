using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Pipeline;

namespace Elders.Cronus
{
    public interface ITransport
    {
        IEnumerable<IConsumerFactory<T>> GetAvailableConsumers<T>(ISerializer serializer, ISubscriptionMiddleware<T> subscriptions, string consumerName);
        IPublisher<TMessage> GetPublisher<TMessage>(ISerializer serializer) where TMessage : IMessage;
    }
}
