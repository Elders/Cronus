namespace NMSD.Cronus.Messaging
{
    /// <summary>
    /// A markup interface telling that the implementing class is a message handler
    /// </summary>
    public interface IMessageHandler { }

    /// <summary>
    /// A markup interface telling that the implementing class will handle all messages of Type <typeparamref name="T"/>
    /// </summary>
    public interface IMessageHandler<T> : IMessageHandler
        where T : IMessage
    {
        void Handle(T message);
    }
}