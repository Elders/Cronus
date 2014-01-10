using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IPort
    {
        IPublisher<ICommand> CommandPublisher { get; set; }
    }
}
