namespace Elders.Cronus.DomainModeling
{
    public interface IPort
    {
        IPublisher<ICommand> CommandPublisher { get; set; }
    }
}