using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Cronus.Transport.InMemory;

namespace NMSD.Cronus.Pipelining.InMemory.Config
{
    public class InMemoryTransportSettings<T> : PipelineTransportSettings<T> where T : ITransportIMessage
    {
        public InMemoryTransportSettings(PipelineSettings pipelineSettings = null)
            : base(pipelineSettings)
        { }

        public override void Build()
        {
            base.Build();

            PipelineFactory = new InMemoryPipelineFactory(PipelineSettings.PipelineNameConvention);
            EndpointFactory = new InMemoryEndpointFactory(PipelineFactory as InMemoryPipelineFactory, PipelineSettings.EndpointNameConvention);
        }
    }
}