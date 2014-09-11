using System;

namespace Elders.Cronus.Pipeline.Transport.InMemory
{
    public class InMemoryPipelineFactory : IPipelineFactory<IPipeline>
    {
        private readonly IPipelineNameConvention pipelineNameConvention;
        private readonly InMemoryPipelineTransport transport;

        public InMemoryPipelineFactory(InMemoryPipelineTransport transport, IPipelineNameConvention pipelineNameConvention)
        {
            this.transport = transport;
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public IPipeline GetPipeline(string pipelineName)
        {
            return transport.GetOrAddPipeline(pipelineName);
        }

        public IPipeline GetPipeline(Type messageType)
        {
            string pipelineName = pipelineNameConvention.GetPipelineName(messageType);
            return GetPipeline(pipelineName);
        }
    }
}