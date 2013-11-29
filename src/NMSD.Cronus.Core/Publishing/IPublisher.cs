using System;

namespace NMSD.Cronus.Core.Publishing
{
    public interface IPublisher<TMessage>
    {
        bool Publish(TMessage message);
    }
}