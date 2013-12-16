using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Eventing
{
    public class InMemoryEventBus : InMemoryBus<MessageCommit, IMessageHandler>
    {
        public override bool Publish(MessageCommit message)
        {
            return base.Publish(message);
        }
    }
}
