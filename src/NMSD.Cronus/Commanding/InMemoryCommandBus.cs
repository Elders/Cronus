using NMSD.Cronus.Messaging;

namespace NMSD.Cronus.Commanding
{
    public class InMemoryCommandBus : InMemoryBus<ICommand, IMessageHandler>
    {

    }
}