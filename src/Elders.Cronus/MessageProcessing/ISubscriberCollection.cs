using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing;

public interface ISubscriberCollection<T>
{
    IEnumerable<ISubscriber> Subscribers { get; }

    IEnumerable<ISubscriber> GetInterestedSubscribers(CronusMessage message);
    void Subscribe(ISubscriber subscriber);
    void UnsubscribeAll();
}
