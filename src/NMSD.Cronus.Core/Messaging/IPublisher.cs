using System;

namespace NMSD.Cronus.Core.Messaging
{
    public interface IPublisher<TMessage>
    {
        bool Publish(TMessage message);
    }
}