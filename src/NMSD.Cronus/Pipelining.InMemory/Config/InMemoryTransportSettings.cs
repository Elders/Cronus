using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Cronus.Transport.InMemory;

namespace NMSD.Cronus.Pipelining.InMemory.Config
{
    public class InMemoryTransportSettings : PipelineTransportSettings
    {
        public override void Build(PipelineSettings pipelineSettings)
        {
            PipelineFactory = new InMemoryPipelineFactory(pipelineSettings.PipelineNameConvention);
            EndpointFactory = new InMemoryEndpointFactory(PipelineFactory as InMemoryPipelineFactory, pipelineSettings.EndpointNameConvention);
        }
    }
}