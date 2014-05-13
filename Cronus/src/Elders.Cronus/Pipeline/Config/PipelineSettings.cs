using System;

namespace Elders.Cronus.Pipeline.Config
{
    public class PipelineSettings : IPipelineSettings
    {
        Func<IPipelineNameConvention> IPipelineSettings.PipelineNameConvention { get; set; }

        Func<IEndpointNameConvention> IPipelineSettings.EndpointNameConvention { get; set; }

        public IPipelineNameConvention BuildPipelineNameConvention()
        {
            return ((IPipelineSettings)this).PipelineNameConvention();
        }

        public IEndpointNameConvention BuildEndpointNameConvention()
        {
            return ((IPipelineSettings)this).EndpointNameConvention();
        }
    }
}
