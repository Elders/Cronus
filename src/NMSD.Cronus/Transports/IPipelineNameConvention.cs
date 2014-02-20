using System;

namespace NMSD.Cronus.Transports
{
    public interface IPipelineNameConvention
    {
        string GetPipelineName(Type messageType);
    }
}