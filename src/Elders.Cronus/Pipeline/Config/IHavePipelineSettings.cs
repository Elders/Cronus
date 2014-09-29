using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Strategy;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IHavePipelineSettings
    {
        Lazy<IPipelineNameConvention> PipelineNameConvention { get; set; }
        Lazy<IEndpointNameConvention> EndpointNameConvention { get; set; }
    }

    public interface IHavePipelineSettings<TContract> : IHavePipelineSettings
    {
    }

    public static class PipelineSettingsExtension
    {
        internal static void CopyPipelineSettingsTo(this IHavePipelineSettings self, IHavePipelineSettings destination)
        {
            destination.PipelineNameConvention = self.PipelineNameConvention;
            destination.EndpointNameConvention = self.EndpointNameConvention;
        }

        public static T WithCommandHandlerEndpointPerBoundedContext<T>(this T self) where T : IHavePipelineSettings<ICommand>
        {
            self.EndpointNameConvention = new Lazy<IEndpointNameConvention>(() => new CommandHandlerEndpointPerBoundedContext(self.PipelineNameConvention.Value));
            return self;
        }

        public static T WithCommandPipelinePerApplication<T>(this T self) where T : IHavePipelineSettings<ICommand>
        {
            self.PipelineNameConvention = new Lazy<IPipelineNameConvention>(() => new CommandPipelinePerApplication());
            return self;
        }

        public static T WithEventPipelinePerApplication<T>(this T self) where T : IHavePipelineSettings<IEvent>
        {
            self.PipelineNameConvention = new Lazy<IPipelineNameConvention>(() => new EventPipelinePerApplication());
            return self;
        }

        public static T WithPortEndpointPerBoundedContext<T>(this T self) where T : IHavePipelineSettings<IEvent>
        {
            self.EndpointNameConvention = new Lazy<IEndpointNameConvention>(() => new PortEndpointPerBoundedContext(self.PipelineNameConvention.Value));
            return self;
        }

        public static T WithProjectionEndpointPerBoundedContext<T>(this T self) where T : IHavePipelineSettings<IEvent>
        {
            self.EndpointNameConvention = new Lazy<IEndpointNameConvention>(() => new ProjectionEndpointPerBoundedContext(self.PipelineNameConvention.Value));
            return self;
        }
    }
}