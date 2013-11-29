using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Publishing;
using Protoreg;

namespace NMSD.Cronus.Core.Commanding
{
    public class RabbitCommandBus : RabbitPublisher<ICommand>
    {
        public RabbitCommandBus(ProtoregSerializer serialiser)
            : base(serialiser)
        {

        }
    }
}
