using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.RabbitMQ;

namespace NMSD.Cronus.Transports
{
    public interface IEndpoint
    {
        string Name { get; }

        void Acknowledge(EndpointMessage message);

        void AcknowledgeAll();

        EndpointMessage BlockDequeue();
       
        bool BlockDequeue(int timeoutInMiliseconds, out EndpointMessage msg);

        void Close();

        EndpointMessage DequeueNoWait();

        void Open();
    }
}
