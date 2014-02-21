using System;
using System.Collections.Generic;
using System.IO;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Multithreading.Work;
using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Cronus.UnitOfWork;
using NMSD.Protoreg;

namespace NMSD.Cronus.EventSourcing
{
    public class EventStoreConsumer
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventStoreConsumer));

        private readonly Type assemblyContainingEventsByEventType;

        private readonly IEventStore eventStore;

        private readonly IPublisher<IEvent> eventPublisher;

        private readonly IEndpointFactory endpointFactory;

        private List<WorkPool> pools;

        private readonly ProtoregSerializer serialiser;

        public EventStoreConsumer(IEndpointFactory endpointFactory, Type assemblyContainingEventsByEventType, ProtoregSerializer serialiser, IEventStore eventStore, IPublisher<IEvent> eventPublisher)
        {
            this.eventPublisher = eventPublisher;
            this.assemblyContainingEventsByEventType = assemblyContainingEventsByEventType;
            this.eventStore = eventStore;
            this.endpointFactory = endpointFactory;
            this.serialiser = serialiser;
        }

        public IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        public void Start(int numberOfWorkers)
        {
            pools = new List<WorkPool>();
            var endpoints = endpointFactory.GetEndpointDefinitions(assemblyContainingEventsByEventType);

            foreach (var endpoint in endpoints)
            {
                var pool = new WorkPool(String.Format("Workpoll {0}", endpoint.EndpointName), numberOfWorkers);
                for (int i = 0; i < numberOfWorkers; i++)
                {
                    pool.AddWork(new ConsumerWork(this, endpointFactory.CreateEndpoint(endpoint)));
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