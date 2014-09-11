using System;
namespace Elders.Cronus.Pipeline.Transport
{
    public interface ITransport
    {
    }

    public interface IPipelineTransport : ITransport, IDisposable
    {
        IEndpointFactory EndpointFactory { get; }

        IPipelineFactory<IPipeline> PipelineFactory { get; }
    }

   
}
