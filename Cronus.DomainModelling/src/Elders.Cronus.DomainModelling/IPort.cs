namespace Elders.Cronus.DomainModelling
{
    public interface IPort
    {
        IPublisher<ICommand> CommandPublisher { get; set; }
    }
}