using System;
using System.Linq;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Config;
using Elders.Protoreg;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusConfiguration
    {
        public CronusConfiguration()
        {
            EventStores = new Dictionary<string, IEventStore>();
            Consumers = new List<IEndpointConsumable>();
        }


        public Dictionary<string, IEventStore> EventStores { get; set; }

        public List<IEndpointConsumable> Consumers { get; set; }

        public ProtoRegistration Protoreg { get; set; }

        public ProtoregSerializer Serializer { get; set; }

        public IPublisher<ICommand> CommandPublisher { get; set; }

        public IPublisher<IEvent> EventPublisher { get; set; }

        public IPublisher<DomainMessageCommit> EventStorePublisher { get; set; }
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
        List<Lazy<IEndpointConsumable>> Consumers { get; set; }
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
            (this as IHaveConsumers).Consumers = new List<Lazy<IEndpointConsumable>>();
        }

        Lazy<IPublisher<ICommand>> IHaveCommandPublisher.CommandPublisher { get; set; }

        List<Lazy<IEndpointConsumable>> IHaveConsumers.Consumers { get; set; }

        Lazy<IPublisher<IEvent>> IHaveEventPublisher.EventPublisher { get; set; }

        Lazy<IPublisher<DomainMessageCommit>> IHaveEventStorePublisher.EventStorePublisher { get; set; }

        Dictionary<string, Lazy<IEventStore>> IHaveEventStores.EventStores { get; set; }

        ProtoregSerializer IHaveSerializer.Serializer { get; set; }

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
                    cronusConfiguration.Consumers = settings.Consumers.Select(x => x.Value).ToList();

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

        public static T UseCommandConsumable<T>(this T self, string boundedContext, Action<CommandConsumableSettings> configure = null) where T : ICronusSettings
        {
            CommandConsumableSettings settings = new CommandConsumableSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UseProjectionConsumable<T>(this T self, string boundedContext, Action<ProjectionConsumableSettings> configure = null) where T : ICronusSettings
        {
            ProjectionConsumableSettings settings = new ProjectionConsumableSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UsePortConsumable<T>(this T self, string boundedContext, Action<PortConsumableSettings> configure = null) where T : ICronusSettings
        {
            PortConsumableSettings settings = new PortConsumableSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UseEventStoreConsumable<T>(this T self, string boundedContext, Action<EventStoreConsumableSettings> configure = null) where T : ICronusSettings
        {
            EventStoreConsumableSettings settings = new EventStoreConsumableSettings();
            self.CopySerializerTo(settings);
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UseDefaultCommandsHost<T>(this T self, string boundedContext, Type assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory)
            where T : ICronusSettings
        {
            self
                .UseCommandConsumable(boundedContext, consumable => consumable
                    .SetMessageThreshold(100, 0)
                    .SetNumberOfConsumers(2)
                    .UseRabbitMqTransport()
                    .CommandConsumer(consumer => consumer
                        .SetConsumeSuccessStrategy(new EndpointPostConsumeStrategy.EventStorePublishEventsOnSuccessPersist((self as IHaveEventPublisher).EventPublisher.Value))
                        .UseCommandHandler(h => h
                            .UseScopeFactory(new ScopeFactory() { CreateBatchScope = () => new RepoBatchScope((self as IHaveEventStores).EventStores[boundedContext].Value.AggregateRepository, (self as IHaveEventStores).EventStores[boundedContext].Value.Persister, self.EventPublisher.Value) })
                            .RegisterAllHandlersInAssembly(assemblyContainingMessageHandlers, messageHandlerFactory))));
            return self;
        }

        public static T WithDefaultPublishers<T>(this T self) where T : ICronusSettings
        {
            self
                .UsePipelineEventPublisher(x => x.UseRabbitMqTransport())
                .UsePipelineCommandPublisher(x => x.UseRabbitMqTransport())
                .UsePipelineEventStorePublisher(x => x.UseRabbitMqTransport());
            return self;
        }
    }

    public class RepoBatchScope : IBatchScope
    {
        IAggregateRepository repository;
        IEventStorePersister persister;
        IPublisher<IEvent> publisher;

        public RepoBatchScope(IAggregateRepository repository, IEventStorePersister persister, IPublisher<IEvent> publisher)
        {
            this.repository = repository;
            this.persister = persister;
            this.publisher = publisher;
        }

        public void Begin()
        {
            Lazy<IAggregateRepository> lazyRepository = new Lazy<IAggregateRepository>(() => new InternalBatchRepository(repository));
            Context.Set<Lazy<IAggregateRepository>>(lazyRepository);
        }

        public void End()
        {
            var currentRepo = Context.Get<Lazy<IAggregateRepository>>().Value as InternalBatchRepository;
            persister.Persist(currentRepo.Commits);
            currentRepo.Commits.ForEach(e => e.Events.ForEach(x => publisher.Publish(x)));
        }

        public IScopeContext Context { get; set; }
    }
}