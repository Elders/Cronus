using System;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Pipeline.Transport.Config
{
    public interface IPipelineTransportSettings : ITransportSettings, IPipelineTransportBuilder
    {
        IPipelineSettings PipelineSettings { get; set; }

        Func<IPipelineTransport> TransportInstance { get; set; }
    }

    public interface IPipelineTransportBuilder
    {
        IPipelineTransport BuildPipelineTransport();
    }

    public class PipelineTransportSettings : IPipelineTransportSettings
    {
        public IEndpointFactory EndpointFactory { get; set; }

        public IPipelineFactory<IPipeline> PipelineFactory { get; set; }

        public IPipelineSettings PipelineSettings { get; set; }

        public Func<IPipelineTransport> TransportInstance { get; set; }

        public virtual IPipelineTransport BuildPipelineTransport()
        {
            throw new NotImplementedException("Unknown transport");
        }
    }
}
