
using Cronus.Core.Eventing;
namespace NMSD.Cronus.Core.Commanding
{
    /// <summary>
    /// A markup interface telling that the implementing class is an event handler
    /// </summary>
    public interface IMessageHandler { }

    /// <summary>
    /// A markup interface telling that the implementing class will handle all events of Type <typeparamref name="T"/>
    /// </summary>
    public interface IMessageHandler<T> : IMessageHandler
        where T : IMessage
    {
        void Handle(T command);
    }
}
