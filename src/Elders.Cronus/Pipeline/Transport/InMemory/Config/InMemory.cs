using System;
using System.Reflection;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.UnitOfWork;
namespace Elders.Cronus.Pipeline.Transport.InMemory.Config
{
    public interface IInMemoryTransportSettings : ISettingsBuilder
    {

    }

    public class InMemoryTransportSettings : IInMemoryTransportSettings
    {
        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;
            var pipelineNameConvention = builder.Container.Resolve<IPipelineNameConvention>(builder.Name);
            var endpointNameConvention = builder.Container.Resolve<IEndpointNameConvention>(builder.Name);
            var transport = new InMemoryPipelineTransport(pipelineNameConvention, endpointNameConvention);
            builder.Container.RegisterSingleton<IPipelineTransport>(() => transport, builder.Name);
        }
    }

    public static class InMemoryTransportExtensions
    {
        public static T UseInMemoryTransport<T>(this T self, Action<InMemoryTransportSettings> configure = null)
        {
            InMemoryTransportSettings settings = new InMemoryTransportSettings();
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
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
            (settings as ISettingsBuilder).Build();
            return self;
        }
        public static T UseInMemoryEventPublisher<T>(this T self, string boundedContext, Action<InMemoryEventPublisherSettings> configure = null) where T : ICronusSettings
        {
            InMemoryEventPublisherSettings settings = new InMemoryEventPublisherSettings();
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
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