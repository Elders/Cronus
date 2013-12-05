using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Commanding
{
    public class InMemoryCommandBus : InMemoryPublisher<ICommand, IMessageHandler>
    {

    }
}