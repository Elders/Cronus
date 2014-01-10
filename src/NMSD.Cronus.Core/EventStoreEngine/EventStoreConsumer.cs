using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NMSD.Cronus.RabbitMQ;
using NMSD.Protoreg;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using NMSD.Cronus.Core.Transports;
using Cronus.Core.EventStore;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.Snapshotting;
using NMSD.Cronus.Core.UnitOfWork;
using NMSD.Cronus.Core.Transports.RabbitMQ;

namespace NMSD.Cronus.Core.Eventing
{
    public class EventStoreConsumer
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventStoreConsumer));

        private readonly Assembly assemblyContainingEvents;

        private readonly IEventStoreEndpointConvention convention;

        private readonly IPersistEventStream eventStore;

        private readonly IPublisher<IEvent> eventPublisher;

        private readonly IEndpointFactory factory;

        private List<WorkPool> pools;

        private readonly ProtoregSerializer serialiser;

        public EventStoreConsumer(IEventStoreEndpointConvention convention, IEndpointFactory factory, Assembly assemblyContainingEvents, ProtoregSerializer serialiser, IPersistEventStream eventStore, IPublisher<IEvent> eventPublisher)
        {
            this.eventPublisher = eventPublisher;
            this.assemblyContainingEvents = assemblyContainingEvents;
            this.eventStore = eventStore;
            this.factory = factory;
            this.convention = convention;
            this.serialiser = serialiser;
        }

        public IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        public void Start(int numberOfWorkers)
        {
            pools = new List<WorkPool>();
            var endpoints = convention.GetEndpointDefinitions(assemblyContainingEvents);

            foreach (var endpoint in endpoints)
            {
                var pool = new WorkPool(String.Format("Workpoll {0}", endpoint.EndpointName), numberOfWorkers);
                for (int i = 0; i < numberOfWorkers; i++)
                {
                    pool.AddWork(new ConsumerWork(this, factory.CreateEndpoint(endpoint)));
                }
                pools.Add(pool);
                pool.StartCrawlers();
            }
        }

        public void Stop()
        {
            foreach (WorkPool pool in pools)
            {
                pool.Stop();
            }
        }

        private class ConsumerWork : IWork
        {
            static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ConsumerWork));
            private EventStoreConsumer consumer;
            private readonly IEndpoint endpoint;
            public ConsumerWork(EventStoreConsumer consumer, IEndpoint endpoint)
            {
                this.endpoint = endpoint;
                this.consumer = consumer;
            }

            public DateTime ScheduledStart { get; set; }

            public void Start()
            {
                IEventStream eventStream = consumer.eventStore.OpenStream();
                endpoint.Open();
                try
                {
                    while (true)
                    {
                        try
                        {
                            eventStream.Clear();
                            for (int i = 0; i < 100; i++)
                            {
                                EndpointMessage rawMessage;
                                if (!endpoint.BlockDequeue(30, out rawMessage))
                                    break;

                                DomainMessageCommit message;
                                using (var stream = new MemoryStream(rawMessage.Body))
                                {
                                    message = consumer.serialiser.Deserialize(stream) as DomainMessageCommit;
                                }
                                eventStream.Events.AddRange(message.Events);
                                eventStream.Snapshots.Add(message.State);
                            }
                            consumer.eventStore.Commit(eventStream);
                            foreach (var @event in eventStream.Events)
                            {
                                consumer.eventPublisher.Publish(@event);
                            }
                            endpoint.AcknowledgeAll();

                        }
                        catch (EndpointClosedException ex)
                        {
                            ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
                            log.Error("Endpoint Closed", ex);
                            break;
                        }
                        catch (PipelineClosedException ex)
                        {
                            ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
                            log.Error("Endpoint Closed", ex);
                            break;
                        }
                    }
                }
                finally
                {
                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
                    eventStream.Dispose();
                    endpoint.Close();
                }

            }

        }
    }
}