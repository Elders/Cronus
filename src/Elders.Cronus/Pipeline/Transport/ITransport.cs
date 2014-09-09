namespace Elders.Cronus.Pipeline.Transport
{
    public interface ITransport
    {
    }

    public interface IPipelineTransport : ITransport
    {
        IEndpointFactory EndpointFactory { get; }

        IPipelineFactory<IPipeline> PipelineFactory { get; }
    }

    public class PipelineTransport : IPipelineTransport
    {
        public PipelineTransport(IPipelineFactory<IPipeline> pipelineFactory, IEndpointFactory endpointFactory)
        {
            this.EndpointFactory = endpointFactory;
            this.PipelineFactory = pipelineFactory;
        }

        public IEndpointFactory EndpointFactory { get; private set; }

        public IPipelineFactory<IPipeline> PipelineFactory { get; private set; }
    }
}
