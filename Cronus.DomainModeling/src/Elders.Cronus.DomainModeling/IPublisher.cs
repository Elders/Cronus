namespace Elders.Cronus.DomainModeling
{
    public interface IPublisher<in TMessage>
        where TMessage : IMessage
    {
        bool Publish(TMessage message);
    }
}