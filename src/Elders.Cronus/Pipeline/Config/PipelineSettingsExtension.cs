using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline.Strategy;
using Elders.Cronus.IocContainer;

namespace Elders.Cronus.Pipeline.Config
{
    public static class PipelineSettingsExtension
    {
        public static T WithCommandHandlerEndpointPerBoundedContext<T>(this T self) where T : IPipelinePublisherSettings<ICommand>
        {
            self.Container.RegisterSingleton<IEndpointNameConvention>(() => new CommandHandlerEndpointPerBoundedContext(self.Container.Resolve<IPipelineNameConvention>(self.Name)), self.Name);
            return self;
        }

        public static T WithCommandPipelinePerApplication<T>(this T self) where T : IPipelinePublisherSettings<ICommand>
        {
            self.Container.RegisterSingleton<IPipelineNameConvention>(() => new CommandPipelinePerApplication(), self.Name);
            return self;
        }

        public static T WithEventPipelinePerApplication<T>(this T self) where T : IPipelinePublisherSettings<IEvent>
        {
            self.Container.RegisterSingleton<IPipelineNameConvention>(() => new EventPipelinePerApplication(), self.Name);
            return self;
        }

        public static T WithPortEndpointPerBoundedContext<T>(this T self) where T : IPipelinePublisherSettings<IEvent>
        {
            self.Container.RegisterSingleton<IEndpointNameConvention>(() => new PortEndpointPerBoundedContext(self.Container.Resolve<IPipelineNameConvention>(self.Name)), self.Name);
            return self;
        }

        public static T WithProjectionEndpointPerBoundedContext<T>(this T self) where T : IPipelinePublisherSettings<IEvent>
        {
            self.Container.RegisterSingleton<IEndpointNameConvention>(() => new ProjectionEndpointPerBoundedContext(self.Container.Resolve<IPipelineNameConvention>(self.Name)), self.Name);
            return self;
        }
    }
}