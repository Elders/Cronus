using System;
using System.Collections.Generic;
using System.Linq;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.EventSourcing.Config;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Protoreg;

namespace NMSD.Cronus.Pipelining.Host.Config
{
    public class CronusGlobalSettings
    {
        public ProtoRegistration protoreg;
        public ProtoregSerializer serializer;
        public Dictionary<IEndpointConsumable, int> consumers = new Dictionary<IEndpointConsumable, int>();
        public Dictionary<string, IEventStore> eventStores = new Dictionary<string, IEventStore>();
        public Dictionary<string, Dictionary<Type, IPublisher>> publishers = new Dictionary<string, Dictionary<Type, IPublisher>>();
    }

    public class CronusConfiguration
    {
        public CronusGlobalSettings GlobalSettings { get; set; }

        List<EventStoreSettings> eventStoreConfigurations = new List<EventStoreSettings>();

        public CronusConfiguration()
        {
            GlobalSettings = new CronusGlobalSettings();
            GlobalSettings.protoreg = new ProtoRegistration();
            GlobalSettings.serializer = new ProtoregSerializer(GlobalSettings.protoreg);
            GlobalSettings.protoreg.RegisterAssembly(typeof(EventBatchWraper));
        }

        public CronusConfiguration ConfigureConsumer<T>(string boundedContextName, Action<PipelineConsumerSettings<T>> configuration) where T : IEndpointConsumer<IMessage>
        {
            var cfg = new PipelineConsumerSettings<T>();
            configuration(cfg);

            IEndpointConsumable consumable = null;

            if (typeof(T) == typeof(EndpointConsumer<ICommand>))
            {
                MessageHandlerCollection<ICommand> handlers = new MessageHandlerCollection<ICommand>();
                foreach (var reg in cfg.Registrations)
                {
                    GlobalSettings.protoreg.RegisterCommonType(reg.Key);
                    foreach (var item in reg.Value)
                    {
                        handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                    }
                }

                var consumer = new EndpointConsumer<ICommand>(handlers, cfg.ScopeFactory, GlobalSettings.serializer);
                consumable = new EndpointConsumable(cfg.Transport.EndpointFactory, consumer);
            }
            else if (typeof(T) == typeof(EndpointConsumer<IEvent>))
            {
                MessageHandlerCollection<IEvent> handlers = new MessageHandlerCollection<IEvent>();
                foreach (var reg in cfg.Registrations)
                {
                    GlobalSettings.protoreg.RegisterCommonType(reg.Key);
                    foreach (var item in reg.Value)
                    {
                        handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                    }
                }

                var consumer = new EndpointConsumer<IEvent>(handlers, cfg.ScopeFactory, GlobalSettings.serializer);
                consumable = new EndpointConsumable(cfg.Transport.EndpointFactory, consumer);
            }
            else
            {
                throw new InvalidOperationException("Unknown consumer.");
            }
            if (cfg.MessagesAssemblies != null)
                cfg.MessagesAssemblies.ToList().ForEach(ass => GlobalSettings.protoreg.RegisterAssembly(ass));

            GlobalSettings.consumers.Add((IEndpointConsumable)consumable, cfg.NumberOfWorkers);
            return this;
        }

        public CronusConfiguration ConfigureEventStoreConsumer<T>(string boundedContextName, Action<PipelineEventStoreConsumerSettings<T>> configuration) where T : EndpointEventStoreConsumer
        {
            var cfg = new PipelineEventStoreConsumerSettings<T>();
            configuration(cfg);

            IEndpointConsumable consumable = null;

            if (typeof(T) == typeof(EndpointEventStoreConsumer))
            {
                var consumer = new EndpointEventStoreConsumer(GlobalSettings.eventStores[boundedContextName], GlobalSettings.publishers[boundedContextName][typeof(IEvent)], GlobalSettings.publishers[boundedContextName][typeof(IEvent)], cfg.AssemblyEventsWhichWillBeIntercepted, GlobalSettings.serializer);
                consumable = new EndpointConsumable(cfg.Transport.EndpointFactory, consumer);
            }
            else
            {
                throw new InvalidOperationException("Unknown consumer.");
            }

            GlobalSettings.consumers.Add((IEndpointConsumable)consumable, cfg.NumberOfWorkers);
            return this;
        }

        public CronusConfiguration ConfigurePublisher<T>(string boundedContextName, Action<PipelinePublisherSettings<T>> configuration) where T : IPublisher
        {
            var cfg = new PipelinePublisherSettings<T>();
            configuration(cfg);

            if (!GlobalSettings.publishers.ContainsKey(boundedContextName))
                GlobalSettings.publishers.Add(boundedContextName, new Dictionary<Type, IPublisher>());

            if (typeof(T) == typeof(PipelinePublisher<ICommand>))
            {
                GlobalSettings.publishers[boundedContextName].Add(typeof(ICommand), new PipelinePublisher<ICommand>(cfg.TransportSettings.PipelineFactory, GlobalSettings.serializer));
            }
            else if (typeof(T) == typeof(PipelinePublisher<IEvent>))
            {
                GlobalSettings.publishers[boundedContextName].Add(typeof(IEvent), new PipelinePublisher<IEvent>(cfg.TransportSettings.PipelineFactory, GlobalSettings.serializer));
            }
            else if (typeof(T) == typeof(PipelinePublisher<DomainMessageCommit>))
            {
                GlobalSettings.publishers[boundedContextName].Add(typeof(DomainMessageCommit), new EventStorePublisher(cfg.TransportSettings.PipelineFactory, GlobalSettings.serializer));
            }
            else
            {
                throw new InvalidOperationException("Unknown consumer.");
            }
            if (cfg.MessagesAssemblies != null)
                cfg.MessagesAssemblies.ToList().ForEach(ass => GlobalSettings.protoreg.RegisterAssembly(ass));
            return this;
        }

        public CronusConfiguration ConfigureEventStore<T>(Action<T> configuration) where T : EventStoreSettings
        {
            T esSettings = Activator.CreateInstance<T>();
            esSettings.GlobalSettings = GlobalSettings;
            configuration(esSettings);
            eventStoreConfigurations.Add(esSettings);
            return this;
        }

        public void Start()
        {
            GlobalSettings.serializer.Build();

            foreach (var esSettings in eventStoreConfigurations)
            {
                var eventStore = esSettings.Build();
                GlobalSettings.protoreg.RegisterAssembly(esSettings.AggregateStatesAssembly);
                GlobalSettings.eventStores.Add(esSettings.BoundedContext, eventStore);
            }

            foreach (var consumer in GlobalSettings.consumers)
            {
                consumer.Key.Start(consumer.Value);
            }
        }

        public void Stop()
        {
            foreach (var consumer in GlobalSettings.consumers)
            {
                consumer.Key.Stop();
            }
        }
    }

}
