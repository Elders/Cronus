

namespace Elders.Cronus.Pipeline.Config
{
    //public static class PipelineSettingsExtension
    //{
    //    public static T WithCommandHandlerEndpointPerBoundedContext<T>(this T self) where T : IPipelinePublisherSettings<ICommand>
    //    {
    //        self.Container.RegisterSingleton<IAppServiceEndpointNameConvention>(() => new AppServiceEndpointPerBoundedContext(self.Container.Resolve<ICommandPipelineNameConvention>(self.Name)), self.Name);
    //        return self;
    //    }

    //    public static T WithCommandPipelinePerApplication<T>(this T self) where T : IPipelinePublisherSettings<ICommand>
    //    {
    //        self.Container.RegisterSingleton<ICommandPipelineNameConvention>(() => new CommandPipelinePerApplication(), self.Name);
    //        return self;
    //    }

    //    public static T WithEventPipelinePerApplication<T>(this T self) where T : IPipelinePublisherSettings<IEvent>
    //    {
    //        self.Container.RegisterSingleton<IEventPipelineNameConvention>(() => new EventPipelinePerApplication(), self.Name);
    //        return self;
    //    }

    //    public static T WithPortEndpointPerBoundedContext<T>(this T self) where T : IPipelinePublisherSettings<IEvent>
    //    {
    //        self.Container.RegisterSingleton<IPortEndpointNameConvention>(() => new PortEndpointPerBoundedContext(self.Container.Resolve<IEventPipelineNameConvention>(self.Name)), self.Name);
    //        return self;
    //    }

    //    public static T WithProjectionEndpointPerBoundedContext<T>(this T self) where T : IPipelinePublisherSettings<IEvent>
    //    {
    //        self.Container.RegisterSingleton<IProjectionEndpointNameConvention>(() => new ProjectionEndpointPerBoundedContext(self.Container.Resolve<IEventPipelineNameConvention>(self.Name)), self.Name);
    //        return self;
    //    }
    //}
}