using System;

namespace Elders.Cronus.MessageProcessing
{
    public interface ISubscriberFactory<T>
    {
        ISubscriber Create(Type handlerType);
    }
}
