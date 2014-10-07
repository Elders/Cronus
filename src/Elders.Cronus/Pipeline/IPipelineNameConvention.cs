using System;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline
{
    public interface IPipelineNameConvention
    {
        string GetPipelineName(Type messageType);
        string GetPipelineName(BoundedContextAttribute boundedContext);
    }

    public interface ICommandPipelineNameConvention : IPipelineNameConvention { }
    public interface IEventPipelineNameConvention : IPipelineNameConvention { }
}