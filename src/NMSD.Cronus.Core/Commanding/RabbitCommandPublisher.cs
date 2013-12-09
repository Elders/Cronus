using NMSD.Cronus.Core.Messaging;
using Protoreg;

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