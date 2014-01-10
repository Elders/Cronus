using System;

namespace NMSD.Cronus.Messaging
{
    public interface IPublisher<TMessage>
    {
        bool Publish(TMessage message);
    }
}