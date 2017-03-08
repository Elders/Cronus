using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Pipeline;

namespace Elders.Cronus
{
    public interface ITransport
    {
        IEnumerable<IConsumerFactory> GetAvailableConsumers(SubscriptionMiddleware subscriptions, string consumerName);
    }
}
