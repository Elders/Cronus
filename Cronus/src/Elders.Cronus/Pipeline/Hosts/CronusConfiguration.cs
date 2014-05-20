using System;
using System.Linq;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Hosts
{
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
            GlobalSettings = new CronusConfiguration();
            (this as IHaveEventStores).EventStores = new Dictionary<string, Lazy<IEventStore>>();
            (this as IHaveConsumers).Consumers = new List<Lazy<IEndpointConsumable>>();
        }

        public CronusConfiguration GlobalSettings { get; set; }

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
        public static T UsePipelineEventPublisher<T>(this T self, Action<EventPipelinePublisherSettings> configure) where T : ICronusSettings
        {
            EventPipelinePublisherSettings settings = new EventPipelinePublisherSettings();
            self.CopySerializerTo(settings);
            configure(settings);
            self.EventPublisher = settings.GetInstanceLazy();
            return self;
        }

        public static T UsePipelineCommandPublisher<T>(this T self, Action<CommandPipelinePublisherSettings> configure) where T : ICronusSettings
        {
            CommandPipelinePublisherSettings settings = new CommandPipelinePublisherSettings();
            self.CopySerializerTo(settings);
            configure(settings);
            self.CommandPublisher = settings.GetInstanceLazy();
            return self;
        }

        public static T UsePipelineEventStorePublisher<T>(this T self, Action<EventStorePipelinePublisherSettings> configure) where T : ICronusSettings
        {
            EventStorePipelinePublisherSettings settings = new EventStorePipelinePublisherSettings();
            self.CopySerializerTo(settings);
            configure(settings);
            self.EventStorePublisher = settings.GetInstanceLazy();
            return self;
        }

        public static T UseCommandConsumable<T>(this T self, string boundedContext, Action<CommandConsumableSettings> configure) where T : ICronusSettings
        {
            CommandConsumableSettings settings = new CommandConsumableSettings();
            self.CopySerializerTo(settings);
            configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UseProjectionConsumable<T>(this T self, string boundedContext, Action<ProjectionConsumableSettings> configure) where T : ICronusSettings
        {
            ProjectionConsumableSettings settings = new ProjectionConsumableSettings();
            self.CopySerializerTo(settings);
            configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UsePortConsumable<T>(this T self, string boundedContext, Action<PortConsumableSettings> configure) where T : ICronusSettings
        {
            PortConsumableSettings settings = new PortConsumableSettings();
            self.CopySerializerTo(settings);
            configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UseEventStoreConsumable<T>(this T self, string boundedContext, Action<EventStoreConsumableSettings> configure) where T : ICronusSettings
        {
            EventStoreConsumableSettings settings = new EventStoreConsumableSettings();
            self.CopySerializerTo(settings);
            configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }
    }
}