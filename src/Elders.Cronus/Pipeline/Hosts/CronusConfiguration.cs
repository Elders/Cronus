using System;
using System.Linq;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Serializer;
using Elders.Cronus.UnitOfWork;
using System.Reflection;

namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusConfiguration : IDisposable
    {
        public CronusConfiguration()
        {
            EventStores = new Dictionary<string, IEventStore>();
            ConsumerHosts = new List<IEndpointConsumerHost>();
        }


        public Dictionary<string, IEventStore> EventStores { get; set; }

        public List<IEndpointConsumerHost> ConsumerHosts { get; set; }

        public ISerializer Serializer { get; set; }

        public IPublisher<ICommand> CommandPublisher { get; set; }

        public IPublisher<IEvent> EventPublisher { get; set; }

        public IPublisher<DomainMessageCommit> EventStorePublisher { get; set; }

        public void Dispose()
        {
            if (EventStores != null)
                foreach (var es in EventStores)
                {
                    var asDisposable = es.Value as IDisposable;
                    if (asDisposable != null)
                        asDisposable.Dispose();
                }
            if (ConsumerHosts != null)
                foreach (var consumer in ConsumerHosts)
                {
                    var asDisposable = consumer as IDisposable;
                    if (asDisposable != null)
                        asDisposable.Dispose();
                }
            if (Serializer != null)
            {
                var asDisposable = Serializer as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
            }
            if (CommandPublisher != null)
            {
                var asDisposable = CommandPublisher as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
            }
            if (EventPublisher != null)
            {
                var asDisposable = EventPublisher as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
            }
            if (EventStorePublisher != null)
            {
                var asDisposable = EventStorePublisher as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
            }
        }
    }

    public interface IHaveCommandPublisher
    {
        Lazy<IPublisher<ICommand>> CommandPublisher { get; set; }
    }

    public interface IHaveEventPublisher
    {
        Lazy<IPublisher<IEvent>> EventPublisher { get; set; }
    }

    public interface IHaveEventStorePublisher
    {
        Lazy<IPublisher<DomainMessageCommit>> EventStorePublisher { get; set; }
    }

    public interface IHaveConsumers
    {
        List<Lazy<IEndpointConsumerHost>> Consumers { get; set; }
    }

    public interface IHaveEventStores
    {
        Dictionary<string, Lazy<IEventStore>> EventStores { get; set; }
    }

    public interface ICronusSettings : ICanConfigureSerializer, IHaveCommandPublisher, IHaveEventPublisher, IHaveEventStorePublisher, IHaveConsumers, IHaveEventStores, ISettingsBuilder<CronusConfiguration>
    {

    }

    public class CronusSettings : ICronusSettings
    {
        public CronusSettings()
        {
            (this as IHaveEventStores).EventStores = new Dictionary<string, Lazy<IEventStore>>();
            (this as IHaveConsumers).Consumers = new List<Lazy<IEndpointConsumerHost>>();
        }

        Lazy<IPublisher<ICommand>> IHaveCommandPublisher.CommandPublisher { get; set; }

        List<Lazy<IEndpointConsumerHost>> IHaveConsumers.Consumers { get; set; }

        Lazy<IPublisher<IEvent>> IHaveEventPublisher.EventPublisher { get; set; }

        Lazy<IPublisher<DomainMessageCommit>> IHaveEventStorePublisher.EventStorePublisher { get; set; }

        Dictionary<string, Lazy<IEventStore>> IHaveEventStores.EventStores { get; set; }

        ISerializer IHaveSerializer.Serializer { get; set; }

        Lazy<CronusConfiguration> ISettingsBuilder<CronusConfiguration>.Build()
        {
            ICronusSettings settings = this as ICronusSettings;

            return new Lazy<CronusConfiguration>(() =>
            {
                var cronusConfiguration = new CronusConfiguration();

                if (settings.CommandPublisher != null)
                    cronusConfiguration.CommandPublisher = settings.CommandPublisher.Value;

                if (settings.EventPublisher != null)
                    cronusConfiguration.EventPublisher = settings.EventPublisher.Value;

                if (settings.EventStorePublisher != null)
                    cronusConfiguration.EventStorePublisher = settings.EventStorePublisher.Value;

                if (settings.Consumers != null && settings.Consumers.Count > 0)
                    cronusConfiguration.ConsumerHosts = settings.Consumers.Select(x => x.Value).ToList();

                if (settings.EventStores != null && settings.EventStores.Count > 0)
                    cronusConfiguration.EventStores = settings.EventStores.ToDictionary(key => key.Key, val => val.Value.Value);

                cronusConfiguration.Serializer = settings.Serializer;

                return cronusConfiguration;
            });
        }
    }

    public static class CronusConfigurationExtensions
    {
        public static T UsePipelineEventPublisher<T>(this T self, Action<EventPipelinePublisherSettings> configure = null) where T : ICronusSettings
        {
            EventPipelinePublisherSettings settings = new EventPipelinePublisherSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.EventPublisher = settings.GetInstanceLazy();
            return self;
        }

        public static T UsePipelineCommandPublisher<T>(this T self, Action<CommandPipelinePublisherSettings> configure = null) where T : ICronusSettings
        {
            CommandPipelinePublisherSettings settings = new CommandPipelinePublisherSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.CommandPublisher = settings.GetInstanceLazy();
            return self;
        }

        public static T UsePipelineEventStorePublisher<T>(this T self, Action<EventStorePipelinePublisherSettings> configure = null) where T : ICronusSettings
        {
            EventStorePipelinePublisherSettings settings = new EventStorePipelinePublisherSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.EventStorePublisher = settings.GetInstanceLazy();
            return self;
        }

        public static T UseCommandConsumerHost<T>(this T self, string boundedContext, Action<CommandConsumerHostSettings> configure = null) where T : ICronusSettings
        {
            CommandConsumerHostSettings settings = new CommandConsumerHostSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UseProjectionConsumerHost<T>(this T self, string boundedContext, Action<ProjectionConsumerHostSettings> configure = null) where T : ICronusSettings
        {
            ProjectionConsumerHostSettings settings = new ProjectionConsumerHostSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UsePortConsumerHost<T>(this T self, string boundedContext, Action<PortConsumerHostSettings> configure = null) where T : ICronusSettings
        {
            PortConsumerHostSettings settings = new PortConsumerHostSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }


        //public static T UseDefaultCommandsHostInMemory<T>(this T self, string boundedContext, Type assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory)
        //    where T : ICronusSettings
        //{
        //    self
        //        .UseCommandConsumerHost(boundedContext, ConsumerHost => ConsumerHost
        //            .SetNumberOfConsumers(2)
        //            .UseInMemoryTransport()
        //            .CommandConsumer(consumer => consumer
        //                .UseCommandHandler(h => h
        //                    .UseUnitOfWork(new UnitOfWorkFactory() { CreateBatchUnitOfWork = () => new ApplicationServiceBatchUnitOfWork((self as IHaveEventStores).EventStores[boundedContext].Value.AggregateRepository, (self as IHaveEventStores).EventStores[boundedContext].Value.Persister, self.EventPublisher.Value) })
        //                    .RegisterAllHandlersInAssembly(assemblyContainingMessageHandlers, messageHandlerFactory))));
        //    return self;
        //}

        //public static T WithDefaultPublishersInMemory<T>(this T self) where T : ICronusSettings
        //{
        //    self
        //        .UsePipelineEventPublisher(x => x.UseInMemoryTransport())
        //        .UsePipelineCommandPublisher(x => x.UseInMemoryTransport())
        //        .UsePipelineEventStorePublisher(x => x.UseInMemoryTransport());
        //    return self;
        //}
        public static T UseInMemoryCommandPublisher<T>(this T self, string boundedContext, Action<InMemoryCommandPublisherSettings> configure = null) where T : ICronusSettings
        {
            InMemoryCommandPublisherSettings settings = new InMemoryCommandPublisherSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.CommandPublisher = settings.GetInstanceLazy();
            return self;
        }
        public static T UseInMemoryEventPublisher<T>(this T self, string boundedContext, Action<InMemoryEventPublisherSettings> configure = null) where T : ICronusSettings
        {
            InMemoryEventPublisherSettings settings = new InMemoryEventPublisherSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.EventPublisher = settings.GetInstanceLazy();
            return self;
        }

        public static T WithDefaultPublishersInMemory<T>(this T self, string boundedContext, Assembly[] assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory, UnitOfWorkFactory eventsHandlersUnitOfWorkFactory)
            where T : ICronusSettings
        {
            self.UseInMemoryCommandPublisher(boundedContext, publisherSettings => publisherSettings
                .UseCommandHandler(handler => handler
                    .UseUnitOfWork(new UnitOfWorkFactory() { CreateBatchUnitOfWork = () => new ApplicationServiceBatchUnitOfWork((self as IHaveEventStores).EventStores[boundedContext].Value.AggregateRepository, (self as IHaveEventStores).EventStores[boundedContext].Value.Persister, self.EventPublisher.Value) })
                    .RegisterAllHandlersInAssembly(assemblyContainingMessageHandlers, messageHandlerFactory)));
            self.UseInMemoryEventPublisher(boundedContext, publisherSettings => publisherSettings
                .UseInMemoryPortAndEventHandler(handler => handler
                    .UseUnitOfWork(eventsHandlersUnitOfWorkFactory)
                    .RegisterAllHandlersInAssembly(assemblyContainingMessageHandlers, messageHandlerFactory)
                ));

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
            var appServiceGateway = new ApplicationServiceGateway(repository, persister);
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
}