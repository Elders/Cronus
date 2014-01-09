using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.RabbitMQ;

namespace NMSD.Cronus.Core.Transports
{
    public interface IEndpoint
    {
        string Name { get; }

        void Acknowledge(EndpointMessage message);

        void AcknowledgeAll();

        EndpointMessage BlockDequeue();

        void Close();

        EndpointMessage DequeueNoWait();

        void Open();
    }
    public interface IPipeline
    {
        void Push(EndpointMessage message);
    }
}
