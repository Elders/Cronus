using System;
using System.Linq;
using System.Collections.Generic;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NMSD.Cronus.Core.Transports;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.UnitOfWork;
using NMSD.Cronus.RabbitMQ;
using NMSD.Protoreg;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using NMSD.Cronus.Core.Transports.RabbitMQ;
using System.IO;

namespace NMSD.Cronus.Core.Eventing
{
    public class EventConsumer : BaseInMemoryConsumer<IEvent, IMessageHandler>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventConsumer));

        private readonly IEventHandlerEndpointConvention convention;

        private readonly IEndpointFactory factory;

        private List<WorkPool> pools;

        private readonly ProtoregSerializer serialiser;

        public EventConsumer(IEventHandlerEndpointConvention convention, IEndpointFactory factory, ProtoregSerializer serialiser)
        {
            this.factory = factory;
            this.convention = convention;
            this.serialiser = serialiser;
        }

        public override void Start(int numberOfWorkers)
        {
            pools = new List<WorkPool>();
            var endpoints = convention.GetEndpointDefinitions(base.RegisteredHandlers.Keys.ToArray());

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

        public override void Stop()
        {
            foreach (WorkPool pool in pools)
            {
                pool.Stop();
            }
        }

        private class ConsumerWork : IWork
        {

            static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ConsumerWork));
            private EventConsumer consumer;
            private readonly IEndpoint endpoint;

            public ConsumerWork(EventConsumer consumer, IEndpoint endpoint)
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
                    while (true)
                    {
                        using (var unitOfWork = consumer.UnitOfWorkFactory.NewBatch())
                        {
                            for (int i = 0; i < 100; i++)
                            {

                                EndpointMessage message;
                                if (endpoint.BlockDequeue(30, out message))
                                {
                                    IEvent @event;
                                    using (var stream = new MemoryStream(message.Body))
                                    {
                                        @event = consumer.serialiser.Deserialize(stream) as IEvent;
                                    }

                                    if (consumer.Handle(@event, unitOfWork))
                                        endpoint.Acknowledge(message);
                                }
                            }
                        }
                    }
                }
                catch (EndpointClosedException ex)
                {
                    log.Error("Endpoint Closed", ex);
                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    endpoint.Close();
                }
            }

        }
    }
}