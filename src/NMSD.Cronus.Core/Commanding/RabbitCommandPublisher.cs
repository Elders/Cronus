using NMSD.Cronus.Core.Messaging;
using NMSD.Protoreg;

namespace NMSD.Cronus.Core.Commanding
{
    public class RabbitCommandPublisher : RabbitPublisher<ICommand>
    {
        public RabbitCommandPublisher(ProtoregSerializer serialiser)
            : base(serialiser)
        {

        }
    }
}