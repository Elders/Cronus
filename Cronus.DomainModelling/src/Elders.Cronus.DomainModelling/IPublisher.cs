namespace Elders.Cronus.DomainModelling
{
    public interface IPublisher<in TMessage>
        where TMessage : IMessage
    {
        bool Publish(TMessage message);
    }
}