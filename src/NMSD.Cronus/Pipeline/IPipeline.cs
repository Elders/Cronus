using System;

namespace NMSD.Cronus.Pipeline
{
    public interface IPipeline : IEquatable<IPipeline>
    {
        string Name { get; }

        void Push(EndpointMessage message);

        void Bind(IEndpoint endpoint);
    }
}