using System;

namespace Elders.Cronus.Pipeline
{
    public interface IPipelineNameConvention
    {
        string GetPipelineName(Type messageType);
    }

    public abstract class PipelineNameConvention : IPipelineNameConvention
    {
        public abstract string GetPipelineName(Type messageType);

        protected abstract string GetCommandsPipelineName(Type messageType);
        protected abstract string GetEventsPipelineName(Type messageType);
    }
}