using System;
using System.Reflection;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.UnitOfWork;
namespace Elders.Cronus.Pipeline.Transport.InMemory.Config
{
    public interface IInMemoryTransportSettings : ISettingsBuilder<IPipelineTransport>, IHavePipelineSettings
    {

    }

    public class InMemoryTransportSettings : IInMemoryTransportSettings
    {
        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        Lazy<IPipelineTransport> ISettingsBuilder<IPipelineTransport>.Build()
        {
            IInMemoryTransportSettings settings = this as IInMemoryTransportSettings;

            return new Lazy<IPipelineTransport>(() => new InMemoryPipelineTransport(settings.PipelineNameConvention.Value, settings.EndpointNameConvention.Value));
        }
    }

    public static class InMemoryTransportExtensions
    {
        public static T UseInMemoryTransport<T>(this T self, Action<InMemoryTransportSettings> configure = null)
                where T : IHaveTransport<IPipelineTransport>, IHavePipelineSettings
        {
            InMemoryTransportSettings transportSettingsInstance = new InMemoryTransportSettings();
            if (configure != null)
                configure(transportSettingsInstance);

            self.Transport = new Lazy<IPipelineTransport>(() =>
            {
                self.CopyPipelineSettingsTo(transportSettingsInstance);
                return transportSettingsInstance.GetInstanceLazy().Value;
            });

            return self;
        }
    }

    public static class CronusConfigurationExtensions
    {

        public static T UseInMemoryCommandPublisher<T>(this T self, string boundedContext, Action<InMemoryCommandPublisherSettings> configure = null) where T : ICronusSettings
        {
            InMemoryCommandPublisherSettings settings = new InMemoryCommandPublisherSettings();
            if (configure != null)
                configure(settings);
            self.CommandPublisher = settings.GetInstanceLazy();
            return self;
        }
        public static T UseInMemoryEventPublisher<T>(this T self, string boundedContext, Action<InMemoryEventPublisherSettings> configure = null) where T : ICronusSettings
        {
            InMemoryEventPublisherSettings settings = new InMemoryEventPublisherSettings();
            if (configure != null)
                configure(settings);
            self.EventPublisher = settings.GetInstanceLazy();
            return self;
        }

        public static T WithDefaultPublishersInMemory<T>(this T self, string boundedContext, Assembly[] assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory, UnitOfWorkFactory eventsHandlersUnitOfWorkFactory)
            where T : ICronusSettings
        {
            //self.UseInMemoryCommandPublisher(boundedContext, publisherSettings => publisherSettings
            //    .UseApplicationServices(handler => handler
            //        .UseUnitOfWork(new UnitOfWorkFactory() { CreateBatchUnitOfWork = () => new ApplicationServiceBatchUnitOfWork((self as IHaveEventStores).EventStores[boundedContext].Value.AggregateRepository, (self as IHaveEventStores).EventStores[boundedContext].Value.Persister, self.EventPublisher.Value) })
            //        .RegisterAllHandlersInAssembly(assemblyContainingMessageHandlers, messageHandlerFactory)));
            //self.UseInMemoryEventPublisher(boundedContext, publisherSettings => publisherSettings
            //    .UsePortsAndProjections(handler => handler
            //        .UseUnitOfWork(eventsHandlersUnitOfWorkFactory)
            //        .RegisterAllHandlersInAssembly(assemblyContainingMessageHandlers, messageHandlerFactory)
            //    ));

            return self;
        }

    }
}