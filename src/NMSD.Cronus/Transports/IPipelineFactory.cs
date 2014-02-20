using System;

namespace NMSD.Cronus.Transports
{
    public interface IPipelineFactory
    {
        IPipeline GetPipeline(string pipelineName);

        IPipeline GetPipeline(Type messageType);
    }
}
