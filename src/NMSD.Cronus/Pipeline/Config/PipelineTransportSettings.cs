namespace NMSD.Cronus.Pipeline.Config
{
    public abstract class PipelineTransportSettings : TransportSettings
    {
        public abstract void Build(PipelineSettings pipelineSettings);

        public IEndpointFactory EndpointFactory { get; protected set; }

        public IPipelineFactory<IPipeline> PipelineFactory { get; protected set; }
    }
}
