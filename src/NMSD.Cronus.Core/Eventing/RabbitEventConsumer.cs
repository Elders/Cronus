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
    public class RabbitEventConsumer : BaseInMemoryConsumer<IEvent, IMessageHandler>
    {
        private readonly IEventHandlerEndpointConvention convention;
        private readonly IEndpointFactory factory;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RabbitEventConsumer));
        private List<WorkPool> pools;
        private readonly ProtoregSerializer serialiser;

        public RabbitEventConsumer(IEventHandlerEndpointConvention convention, IEndpointFactory factory, ProtoregSerializer serialiser)
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
            private RabbitEventConsumer consumer;
            private readonly IEndpoint endpoint;

            public ConsumerWork(RabbitEventConsumer consumer, IEndpoint endpoint)
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