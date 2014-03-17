using System;
using NMSD.Cronus.Pipelining;

namespace NMSD.Cronus.Transports.RabbitMQ
{
    public interface IRabbitMqPipeline : IPipeline
    {
        void Open();

        void Close();

        void Declare();
    }
}
