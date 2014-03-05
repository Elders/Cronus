using System;
using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Protoreg;
using System.Linq;

namespace NMSD.Cronus.Sample.Player
{
    public class CronusConfiguration
    {
        ProtoRegistration protoreg;
        ProtoregSerializer serializer;
        Dictionary<IEndpointConsumable, int> consumers = new Dictionary<IEndpointConsumable, int>();
        public Dictionary<string, IEventStore> eventStores = new Dictionary<string, IEventStore>();
        public Dictionary<string, Dictionary<Type, IPublisher>> publishers = new Dictionary<string, Dictionary<Type, IPublisher>>();

        public CronusConfiguration()
        {
            protoreg = new ProtoRegistration();
            serializer = new ProtoregSerializer(protoreg);
            protoreg.RegisterAssembly(typeof(Wraper));
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
                    protoreg.RegisterCommonType(reg.Key);
                    handlers.RegisterHandler(reg.Key, reg.Value.Item1, reg.Value.Item2);
                }

                var consumer = new EndpointConsumer<ICommand>(handlers, cfg.ScopeFactory, serializer);
                consumable = new EndpointConsumable(cfg.Transport.EndpointFactory, consumer);
            }
            else if (typeof(T) == typeof(EndpointConsumer<IEvent>))
            {
                MessageHandlerCollection<IEvent> handlers = new MessageHandlerCollection<IEvent>();
                foreach (var reg in cfg.Registrations)
                {
                    protoreg.RegisterCommonType(reg.Key);
                    handlers.RegisterHandler(reg.Key, reg.Value.Item1, reg.Value.Item2);
                }

                var consumer = new EndpointConsumer<IEvent>(handlers, cfg.ScopeFactory, serializer);
                consumable = new EndpointConsumable(cfg.Transport.EndpointFactory, consumer);
            }
            else
            {
                throw new InvalidOperationException("Unknown consumer.");
            }
            if (cfg.MessagesAssemblies != null)
                cfg.MessagesAssemblies.ToList().ForEach(ass => protoreg.RegisterAssembly(ass));

            consumers.Add((IEndpointConsumable)consumable, cfg.NumberOfWorkers);
            return this;
        }

        public CronusConfiguration ConfigureEventStoreConsumer<T>(string boundedContextName, Action<PipelineEventStoreConsumerSettings<T>> configuration) where T : EndpointEventStoreConsumer
        {
            var cfg = new PipelineEventStoreConsumerSettings<T>();
            configuration(cfg);

            IEndpointConsumable consumable = null;

            if (typeof(T) == typeof(EndpointEventStoreConsumer))
            {
                var consumer = new EndpointEventStoreConsumer(eventStores[boundedContextName], publishers[boundedContextName][typeof(IEvent)], publishers[boundedContextName][typeof(IEvent)], cfg.AssemblyEventsWhichWillBeIntercepted, serializer);
                consumable = new EndpointConsumable(cfg.Transport.EndpointFactory, consumer);
            }
            else
            {
                throw new InvalidOperationException("Unknown consumer.");
            }

            consumers.Add((IEndpointConsumable)consumable, cfg.NumberOfWorkers);
            return this;
        }

        public CronusConfiguration ConfigurePublisher<T>(string boundedContextName, Action<PipelinePublisherSettings<T>> configuration) where T : IPublisher
        {
            var cfg = new PipelinePublisherSettings<T>();
            configuration(cfg);

            if (!publishers.ContainsKey(boundedContextName))
                publishers.Add(boundedContextName, new Dictionary<Type, IPublisher>());

            if (typeof(T) == typeof(PipelinePublisher<ICommand>))
            {
                publishers[boundedContextName].Add(typeof(ICommand), new PipelinePublisher<ICommand>(cfg.Transport.PipelineFactory, serializer));
            }
            else if (typeof(T) == typeof(PipelinePublisher<IEvent>))
            {
                publishers[boundedContextName].Add(typeof(IEvent), new PipelinePublisher<IEvent>(cfg.Transport.PipelineFactory, serializer));
            }
            else if (typeof(T) == typeof(PipelinePublisher<DomainMessageCommit>))
            {
                publishers[boundedContextName].Add(typeof(DomainMessageCommit), new EventStorePublisher(cfg.Transport.PipelineFactory, serializer));
            }
            else
            {
                throw new InvalidOperationException("Unknown consumer.");
            }
            if (cfg.MessagesAssemblies != null)
                cfg.MessagesAssemblies.ToList().ForEach(ass => protoreg.RegisterAssembly(ass));
            return this;
        }


        public CronusConfiguration ConfigureEventStore<T>(Action<EventStoreSettings> configuration) where T : IEventStore
        {
            var cfg = new EventStoreSettings();
            configuration(cfg);

            MssqlEventStore eventStore = null;

            if (typeof(T) == typeof(MssqlEventStore))
                eventStore = new MssqlEventStore(cfg.BoundedContext, cfg.ConnectionString, serializer);
            else
                throw new InvalidOperationException("Unknown Event store.");

            protoreg.RegisterAssembly(cfg.AggregateStatesAssembly);

            eventStores.Add(cfg.BoundedContext, eventStore);
            return this;
        }

        public void Start()
        {
            serializer.Build();
            foreach (var consumer in consumers)
            {
                consumer.Key.Start(consumer.Value);
            }
        }
    }
}
