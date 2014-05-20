using System;

namespace Elders.Cronus.Pipeline.Transport.InMemory.Config
{
    public class InMemory
    {
        //public override void Build(PipelineSettings pipelineSettings)
        //{
        //    //PipelineFactory = new InMemoryPipelineFactory(pipelineSettings.PipelineNameConvention);
        //    //EndpointFactory = new InMemoryEndpointFactory(PipelineFactory as InMemoryPipelineFactory, pipelineSettings.EndpointNameConvention);
        //}
        public IPipelineTransport BuildPipelineTransport()
        {
            throw new NotImplementedException();
        }
    }
}