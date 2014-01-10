using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.RabbitMQ;

namespace NMSD.Cronus.Core.Transports
{
    public interface IPipeline
    {
        void Push(EndpointMessage message);
    }
}
