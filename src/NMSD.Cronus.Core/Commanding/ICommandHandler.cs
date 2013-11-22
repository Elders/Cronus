namespace NMSD.Cronus.Core.Commanding
{
    /// <summary>
    /// A markup interface telling that the implementing class is an event handler
    /// </summary>
    public interface ICommandHandler { }

    /// <summary>
    /// A markup interface telling that the implementing class will handle all events of Type <typeparamref name="T"/>
    /// </summary>
    public interface ICommandHandler<T>
        where T : ICommand
    {
        void Handle(T command);
    }
}
