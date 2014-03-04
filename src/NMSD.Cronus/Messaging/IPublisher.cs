using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Messaging
{
    public interface IPublisher : ITransportIMessage
    {
        bool Publish(IMessage message);
    }

    public interface IPublisher<in TMessage> : IPublisher
        where TMessage : IMessage
    {
        bool Publish(TMessage message);
    }
}