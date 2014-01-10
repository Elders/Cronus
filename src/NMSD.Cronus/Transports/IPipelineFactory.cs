using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.RabbitMQ;

namespace NMSD.Cronus.Transports
{
    public interface IPipelineFactory
    {
        IPipeline GetPipeline(string pipelineName);
    }
}
