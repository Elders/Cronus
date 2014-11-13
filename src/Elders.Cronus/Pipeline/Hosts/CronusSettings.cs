using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Serializer;
using Elders.Cronus.UnitOfWork;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.EventSourcing.InMemory.Config;

namespace Elders.Cronus.Pipeline.Hosts
{
    public interface ICronusSettings : ICanConfigureSerializer, ISettingsBuilder
    {

    }

    public class CronusSettings : ICronusSettings
    {
        public CronusSettings(IContainer container)
        {
            (this as ISettingsBuilder).Container = container;
        }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;
            var consumers = builder.Container.ResolveAll<IEndpointConsumer>();
            CronusHost host = new CronusHost();
            host.Consumers = consumers;
            builder.Container.RegisterSingleton(typeof(CronusHost), () => host);
        }

        //Lazy<CronusHost> ISettingsBuilder<CronusHost>.Build()
        //{
        //    ICronusSettings settings = this as ICronusSettings;

        //    return new Lazy<CronusHost>(() =>
        //    {
        //        var cronusConfiguration = new CronusHost();

        //        if (settings.CommandPublisher != null)
        //            cronusConfiguration.CommandPublisher = settings.CommandPublisher.Value;

        //        if (settings.EventPublisher != null)
        //            cronusConfiguration.EventPublisher = settings.EventPublisher.Value;

        //        if (settings.Consumers != null && settings.Consumers.Count > 0)
        //            cronusConfiguration.Consumers = settings.Consumers.Select(x => x.Value).ToList();

        //        cronusConfiguration.Serializer = settings.Serializer;

        //        return cronusConfiguration;
        //    });
    }
}

public static class CronusConfigurationExtensions
{
    public static T UsePipelineEventPublisher<T>(this T self, Action<EventPipelinePublisherSettings> configure = null) where T : IConsumerSettings
    {
        return UsePipelineEventPublisher(self, null, configure);
    }

    public static T UsePipelineEventPublisher<T>(this T self, string name, Action<EventPipelinePublisherSettings> configure = null) where T : IConsumerSettings
    {
        EventPipelinePublisherSettings settings = new EventPipelinePublisherSettings(self, name);
        if (configure != null)
            configure(settings);
        (settings as ISettingsBuilder).Build();
        return self;
    }

    public static T UsePipelineCommandPublisher<T>(this T self, Action<CommandPipelinePublisherSettings> configure = null) where T : IConsumerSettings
    {
        return UsePipelineCommandPublisher(self, null, configure);
    }

    public static T UsePipelineCommandPublisher<T>(this T self, string name, Action<CommandPipelinePublisherSettings> configure = null) where T : IConsumerSettings
    {
        CommandPipelinePublisherSettings settings = new CommandPipelinePublisherSettings(self, name);
        if (configure != null)
            configure(settings);
        (settings as ISettingsBuilder).Build();
        return self;
    }

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

    public static T UseInMemoryEventStore<T>(this T self) where T : ICronusSettings
    {
        InMemoryEventStoreSettings settings = new InMemoryEventStoreSettings(self);
        (settings as ISettingsBuilder).Build();
        return self;
    }
}

public class ApplicationServiceBatchUnitOfWork : IBatchUnitOfWork
{
    IAggregateRepository repository;
    IEventStorePersister persister;
    IPublisher<IEvent> publisher;

    public ApplicationServiceBatchUnitOfWork(IAggregateRepository repository, IEventStorePersister persister, IPublisher<IEvent> publisher)
    {
        this.repository = repository;
        this.persister = persister;
        this.publisher = publisher;
    }

    public void Begin()
    {
        var appServiceGateway = new ApplicationServiceGateway(repository);
        Lazy<IAggregateRepository> lazyRepository = new Lazy<IAggregateRepository>(() => (appServiceGateway));
        Lazy<IApplicationServiceGateway> gateway = new Lazy<IApplicationServiceGateway>(() => (appServiceGateway));
        Context.Set<Lazy<IAggregateRepository>>(lazyRepository);
        Context.Set<Lazy<IApplicationServiceGateway>>(gateway);
    }

    public void End()
    {
        var gateway = Context.Get<Lazy<IApplicationServiceGateway>>().Value;
        gateway.CommitChanges(@event => publisher.Publish(@event));
    }

    public IUnitOfWorkContext Context { get; set; }
}
