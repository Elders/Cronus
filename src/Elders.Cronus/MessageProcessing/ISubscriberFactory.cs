using System;

namespace Elders.Cronus.MessageProcessing;

public interface ISubscriberFactory<out T>
{
    ISubscriber Create(Type handlerType);
}
