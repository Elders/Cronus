using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Multithreading.Work;
using NMSD.Cronus.RabbitMQ;
using NMSD.Protoreg;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.UnitOfWork;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.EventSourcing
{
    public class EventStoreConsumer
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventStoreConsumer));

        private readonly Assembly assemblyContainingEvents;

        private readonly IEventStoreEndpointConvention convention;

        private readonly IEventStore eventStore;

        private readonly IPublisher<IEvent> eventPublisher;

        private readonly IEndpointFactory factory;

        private List<WorkPool> pools;

        private readonly ProtoregSerializer serialiser;

        public EventStoreConsumer(IEventStoreEndpointConvention convention, IEndpointFactory factory, Assembly assemblyContainingEvents, ProtoregSerializer serialiser, IEventStore eventStore, IPublisher<IEvent> eventPublisher)
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
                try
                {
                    endpoint.Open();
                    consumer.eventStore.UseStream(() =>
                        {
                            EndpointMessage rawMessage;
                            DomainMessageCommit commit = null;
                            if (endpoint.BlockDequeue(30, out rawMessage))
                            {
                                using (var stream = new MemoryStream(rawMessage.Body))
                                {
                                    commit = consumer.serialiser.Deserialize(stream) as DomainMessageCommit;
                                }
                            }
                            return commit;
                        },
                        (eventStream, commit) => commit == null || eventStream.Events.Count == 100,
                        eventStream =>
                        {
                            foreach (var @event in eventStream.Events)
                            {
                                consumer.eventPublisher.Publish(@event);
                            }
                            endpoint.AcknowledgeAll();
                        },
                        eventStream => false);
                }
                catch (EndpointClosedException ex)
                {
                    log.Error("Endpoint Closed", ex);
                }
                catch (PipelineClosedException ex)
                {
                    log.Error("Endpoint Closed", ex);
                }
                finally
                {
                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
                    endpoint.Close();
                }
            }
        }
    }
}