using NMSD.Cronus.Pipeline.Config;

namespace NMSD.Cronus.Pipeline.Transport.InMemory.Config
{
    public class InMemory : PipelineTransportSettings
    {
        public override void Build(PipelineSettings pipelineSettings)
        {
            PipelineFactory = new InMemoryPipelineFactory(pipelineSettings.PipelineNameConvention);
            EndpointFactory = new InMemoryEndpointFactory(PipelineFactory as InMemoryPipelineFactory, pipelineSettings.EndpointNameConvention);
        }
    }
}