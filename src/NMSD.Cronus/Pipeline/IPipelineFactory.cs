using System;

namespace NMSD.Cronus.Pipeline
{
    public interface IPipelineFactory<out T> where T : IPipeline
    {
        T GetPipeline(string pipelineName);

        T GetPipeline(Type messageType);
    }
}
