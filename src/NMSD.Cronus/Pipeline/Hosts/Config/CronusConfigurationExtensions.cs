using System;
using NMSD.Cronus.EventSourcing.Config;
using NMSD.Cronus.Pipelining.Config;

namespace NMSD.Cronus.Pipelining.Hosts.Config
{
    public static class CronusConfigurationExtensions
    {
        public static CronusConfiguration PipelineEventPublisher(this CronusConfiguration config, Action<PipelineEventPublisherSettings> configure)
        {
            PipelineEventPublisherSettings settings = new PipelineEventPublisherSettings();
            settings.GlobalSettings = config.GlobalSettings;
            configure(settings);
            config.EventPublisherConfiguration = settings;

            return config;
        }

        public static CronusConfiguration PipelineCommandPublisher(this CronusConfiguration config, Action<PipelineCommandPublisherSettings> configure)
        {
            PipelineCommandPublisherSettings settings = new PipelineCommandPublisherSettings();
            settings.GlobalSettings = config.GlobalSettings;
            configure(settings);
            config.CommandPublisherConfiguration = settings;

            return config;
        }

        public static CronusConfiguration PipelineEventStorePublisher(this CronusConfiguration config, Action<PipelineEventStorePublisherSettings> configure)
        {
            PipelineEventStorePublisherSettings settings = new PipelineEventStorePublisherSettings();
            settings.GlobalSettings = config.GlobalSettings;
            configure(settings);
            config.EventStorePublisherConfiguration = settings;

            return config;
        }

        public static CronusConfiguration ConfigureEventStore<T>(this CronusConfiguration config, Action<T> configure) where T : EventStoreSettings
        {
            T settings = Activator.CreateInstance<T>();
            settings.GlobalSettings = config.GlobalSettings;
            configure(settings);
            config.EventStoreConfigurations.Add(settings);

            return config;
        }

        public static CronusConfiguration ConfigureConsumer<T>(this CronusConfiguration config, string boundedContextName, Action<T> configuration) where T : IEndpointConsumerSetting
        {
            var cfg = Activator.CreateInstance<T>();
            cfg.BoundedContext = boundedContextName;
            cfg.GlobalSettings = config.GlobalSettings;
            configuration(cfg);
            config.ConsumersConfigurations.Add(cfg);

            return config;
        }
    }
}
