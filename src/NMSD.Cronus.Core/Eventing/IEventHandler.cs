namespace NMSD.Cronus.Core.Eventing
{
    /// <summary>
    /// A markup interface telling that the implementing class is an event handler
    /// </summary>
    public interface IEventHandler { }

    /// <summary>
    /// A markup interface telling that the implementing class will handle all events of Type <typeparamref name="T"/>
    /// </summary>
    public interface IEventHandler<T>
        where T : IEvent
    {
        void Handle(T evnt);
    }
}