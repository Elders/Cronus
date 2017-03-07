using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Pipeline;
using Elders.Cronus.Serializer;

namespace Elders.Cronus
{
    public interface ITransport
    {
        IEnumerable<BaseConsumerWork> GetWorkToConsumeFor(SubscriptionMiddleware subscriptions, ISerializer serializer, string consumerName);
    }
}
