using System;
using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Protoreg;

namespace NMSD.Cronus.Sample.Player
{
    public class CronusConfiguration
    {
        ProtoRegistration protoreg;
        ProtoregSerializer serializer;
        Dictionary<IStartableConsumer<IMessage>, int> consumers = new Dictionary<IStartableConsumer<IMessage>, int>();
        Dictionary<string, IEventStore> eventStores = new Dictionary<string, IEventStore>();
        public Publisher<ICommand> Publisher { get; set; }

        public CronusConfiguration()
        {
            protoreg = new ProtoRegistration();
            serializer = new ProtoregSerializer(protoreg);
        }

        public CronusConfiguration ConfigureConsumer<T>(Action<PipelineConsumerSettings<T>> configuration) where T : IStartableConsumer<IMessage>
        {
            var cfg = new PipelineConsumerSettings<T>();
            configuration(cfg);

            ITransportIMessage consumer = null;

            if (typeof(T) == typeof(PipelineConsumer<ICommand>))
            {
                MessageHandlerCollection<ICommand> handlers = new MessageHandlerCollection<ICommand>();
                foreach (var reg in cfg.Registrations)
                {
                    protoreg.RegisterCommonType(reg.Key);
                    handlers.RegisterHandler(reg.Key, reg.Value.Item1, reg.Value.Item2);
                }

                consumer = new PipelineConsumer<ICommand>(cfg.Transport.EndpointFactory, serializer, handlers, cfg.ScopeFactory);
            }
            else
            {
                throw new InvalidOperationException("Unknown consumer.");
            }


            consumers.Add((IStartableConsumer<IMessage>)consumer, cfg.NumberOfWorkers);
            return this;
        }

        public CronusConfiguration ConfigurePublisher<T>(Action<PipelinePublisherSettings<T>> configuration) where T : IPublisher
        {
            var cfg = new PipelinePublisherSettings<T>();
            configuration(cfg);

            if (typeof(T) == typeof(Publisher<ICommand>))
            {
                Publisher = new PipelinePublisher<ICommand>(cfg.Transport.PipelineFactory, serializer);

            }
            else
            {
                throw new InvalidOperationException("Unknown consumer.");
            }

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
