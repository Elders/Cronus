using System;

namespace Elders.Cronus.Pipeline
{
    public interface IPipeline : IEquatable<IPipeline>
    {
        string Name { get; }

        void Push(EndpointMessage message);

        void Bind(IEndpoint endpoint);
    }
}