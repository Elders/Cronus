using System.Collections.Generic;
using Elders.Cronus.MessageProcessingMiddleware;

namespace Elders.Cronus
{
    public interface IMessageProcessor
    {
        string Name { get; }

        IEnumerable<SubscriberMiddleware> GetSubscriptions();

        IFeedResult Run(List<CronusMessage> context);
    }
}
