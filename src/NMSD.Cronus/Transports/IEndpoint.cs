using System;
using System.Collections.Generic;
namespace NMSD.Cronus.Transports
{
    public interface IEndpoint : IEquatable<IEndpoint>
    {
        IDictionary<string, object> RoutingHeaders { get; }

        string RoutingKey { get; }

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