using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Transport;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusConfiguration
    {
        public List<IEndpointConsumerSetting<IMessage>> ConsumersConfigurations = new List<IEndpointConsumerSetting<IMessage>>();

        public List<EventStoreSettings> EventStoreConfigurations = new List<EventStoreSettings>();

        public CronusConfiguration()
        {
            GlobalSettings = new CronusGlobalSettings();
            GlobalSettings.Protoreg = new ProtoRegistration();
            //GlobalSettings.Protoreg.RegisterCommonType<ProtoregSerializableException>();
            GlobalSettings.Serializer = new ProtoregSerializer(GlobalSettings.Protoreg);
            GlobalSettings.Protoreg.RegisterAssembly(typeof(EventBatchWraper));
            GlobalSettings.Protoreg.RegisterAssembly(typeof(IMessage));
        }

        public PipelineCommandPublisherSettings CommandPublisherConfiguration { get; set; }

        public PipelineEventPublisherSettings EventPublisherConfiguration { get; set; }

        public PipelineEventStorePublisherSettings EventStorePublisherConfiguration { get; set; }

        public CronusGlobalSettings GlobalSettings { get; set; }

        public CronusConfiguration Build()
        {
            if (CommandPublisherConfiguration != null)
                GlobalSettings.CommandPublisher = CommandPublisherConfiguration.Build();

            if (EventPublisherConfiguration != null)
                GlobalSettings.EventPublisher = EventPublisherConfiguration.Build();

            if (EventStorePublisherConfiguration != null)
                GlobalSettings.EventStorePublisher = EventStorePublisherConfiguration.Build();

            foreach (var esSettings in EventStoreConfigurations)
            {
                GlobalSettings.EventStorePersisters[esSettings.BoundedContext] = esSettings.BuildEventStorePersister();
                GlobalSettings.AggregateRepositories[esSettings.BoundedContext] = esSettings.BuildAggregateRepository();
                GlobalSettings.EventStorePlayers[esSettings.BoundedContext] = esSettings.BuildEventStorePlayer();
                GlobalSettings.EventStoreHandlers[esSettings.BoundedContext] = esSettings.BuildEventStoreHandlers();
            }

            foreach (var consumerSettings in ConsumersConfigurations)
            {
                IEndpointConsumable<IMessage> consumable = consumerSettings.Build();
                GlobalSettings.Consumers.Add(consumable);
            }

            GlobalSettings.Serializer.Build();

            return this;
        }
    }
}