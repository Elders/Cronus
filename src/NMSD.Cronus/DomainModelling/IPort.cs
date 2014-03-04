using NMSD.Cronus.Messaging;

namespace NMSD.Cronus.DomainModelling
{
    public interface IPort
    {
        IPublisher<ICommand> CommandPublisher { get; set; }
    }
}