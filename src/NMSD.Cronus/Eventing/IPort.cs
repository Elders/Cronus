using NMSD.Cronus.Commanding;
using NMSD.Cronus.Messaging;

namespace NMSD.Cronus.Eventing
{
    public interface IPort
    {
        IPublisher<ICommand> CommandPublisher { get; set; }
    }
}
