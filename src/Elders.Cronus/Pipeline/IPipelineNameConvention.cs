using System;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline
{
    public interface IPipelineNameConvention
    {
        string GetPipelineName(Type messageType);
        string GetPipelineName(BoundedContextAttribute boundedContext);
    }
}