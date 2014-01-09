using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Protoreg;

namespace NMSD.Cronus.Core.Eventing
{
    public class RabbitEventPublisher : RabbitPublisher<IEvent>
    {
        public RabbitEventPublisher(ProtoregSerializer serialiser)
            : base(serialiser)
        {

        }
    }

}