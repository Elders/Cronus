using System;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Pipeline
{
    public interface IPipelineNameConvention
    {
        string GetPipelineName(Type messageType);
        string GetPipelineName(BoundedContextAttribute boundedContext);
    }
}