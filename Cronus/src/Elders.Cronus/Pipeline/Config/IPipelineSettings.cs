using System;
using Elders.Cronus.Pipeline.Strategy;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IPipelineBuilder
    {
        IPipelineNameConvention BuildPipelineNameConvention();

        IEndpointNameConvention BuildEndpointNameConvention();
    }

    public interface IPipelineSettings : IPipelineBuilder
    {
        Func<IPipelineNameConvention> PipelineNameConvention { get; set; }

        Func<IEndpointNameConvention> EndpointNameConvention { get; set; }
    }

    public static class PipelineSettingsExtension
    {
        public static IPipelineSettings UseCommandHandlerEndpointPerBoundedContext(this IPipelineSettings self)
        {
            self.EndpointNameConvention = () => new CommandHandlerEndpointPerBoundedContext(self.PipelineNameConvention());
            return self;
        }

        public static IPipelineSettings UseCommandPipelinePerApplication(this IPipelineSettings self)
        {
            self.PipelineNameConvention = () => new CommandPipelinePerApplication();
            return self;
        }

        public static IPipelineSettings UseEventPipelinePerApplication(this IPipelineSettings self)
        {
            self.PipelineNameConvention = () => new EventPipelinePerApplication();
            return self;
        }

        public static IPipelineSettings UseEventStoreEndpointPerBoundedContext(this IPipelineSettings self)
        {
            self.EndpointNameConvention = () => new EventStoreEndpointPerBoundedContext(self.PipelineNameConvention());
            return self;
        }

        public static IPipelineSettings UseEventStorePipelinePerApplication(this IPipelineSettings self)
        {
            self.PipelineNameConvention = () => new EventStorePipelinePerApplication();
            return self;
        }

        public static IPipelineSettings UsePortEndpointPerBoundedContext(this IPipelineSettings self)
        {
            self.EndpointNameConvention = () => new PortEndpointPerBoundedContext(self.PipelineNameConvention());
            return self;
        }

        public static IPipelineSettings UseProjectionEndpointPerBoundedContext(this IPipelineSettings self)
        {
            self.EndpointNameConvention = () => new ProjectionEndpointPerBoundedContext(self.PipelineNameConvention());
            return self;
        }
    }
}
