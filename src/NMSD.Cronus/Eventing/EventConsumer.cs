using System;
using System.Linq;
using System.Collections.Generic;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Multithreading.Work;
using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.UnitOfWork;
using NMSD.Cronus.RabbitMQ;
using NMSD.Protoreg;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using NMSD.Cronus.Transports.RabbitMQ;
using System.IO;
using System.Reflection;
using NMSD.Cronus.Commanding;

namespace NMSD.Cronus.Eventing
{
    public class EventConsumer : BaseInMemoryConsumer<IEvent, IMessageHandler>
    {
        private readonly IPublisher<ICommand> commandPublisher;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventConsumer));

        private readonly IEventHandlerEndpointConvention convention;

        private readonly IEndpointFactory factory;

        private List<WorkPool> pools;

        private readonly ProtoregSerializer serialiser;

        public EventConsumer(IEventHandlerEndpointConvention convention, IEndpointFactory factory, ProtoregSerializer serialiser, IPublisher<ICommand> commandPublisher)
        {
            this.commandPublisher = commandPublisher;
            this.factory = factory;
            this.convention = convention;
            this.serialiser = serialiser;
        }

        public void RegisterAllHandlersInAssembly(Assembly assemblyContainingMessageHandlers)
        {
            RegisterAllHandlersInAssembly(assemblyContainingMessageHandlers, x => (IMessageHandler)FastActivator.CreateInstance(x));
        }
        public void RegisterAllHandlersInAssembly(Assembly assemblyContainingMessageHandlers, Func<Type, IMessageHandler> messageHandlerFactory)
        {
            MessageHandlerRegistrations.RegisterAllHandlersInAssembly<IMessageHandler>(this, assemblyContainingMessageHandlers, x =>
            {
                var handler = FastActivator.CreateInstance(x);
                var port = handler as IPort;
                if (port != null)
                    port.CommandPublisher = commandPublisher;

                return (port ?? handler) as IMessageHandler;
            });
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