using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Multithreading.Work;
using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Protoreg;

namespace NMSD.Cronus.Commanding
{
    public class CommandConsumer : BaseInMemoryConsumer<ICommand, IMessageHandler>
    {
        private readonly IAggregateRepository eventStore;
        private readonly IEndpointFactory endpointFactory;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CommandConsumer));
        private List<WorkPool> pools;
        private readonly ProtoregSerializer serialiser;

        public CommandConsumer(IEndpointFactory endpointFactory, ProtoregSerializer serialiser, IAggregateRepository eventStore)
        {
            this.eventStore = eventStore;
            this.endpointFactory = endpointFactory;
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
                var handler = messageHandlerFactory(x);
                (handler as IAggregateRootApplicationService).Repository = eventStore;
                return handler;
            });
        }

        public override void Start(int numberOfWorkers)
        {
            pools = new List<WorkPool>();
            var endpointDefinitions = endpointFactory.GetEndpointDefinitions(base.RegisteredHandlers.Keys.ToArray());

            foreach (var endpointDefinition in endpointDefinitions)
            {
                var pool = new WorkPool(String.Format("Workpoll {0}", endpointDefinition.EndpointName), numberOfWorkers);
                for (int i = 0; i < numberOfWorkers; i++)
                {
                    pool.AddWork(new ConsumerWork(this, endpointFactory.CreateEndpoint(endpointDefinition)));
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
            private CommandConsumer consumer;
            private readonly IEndpoint endpoint;
            public ConsumerWork(CommandConsumer consumer, IEndpoint endpoint)
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
                        using (var batchedUoW = consumer.UnitOfWorkFactory.NewBatch())
                        {
                            var rawMessage = endpoint.BlockDequeue();

                            ICommand command;
                            using (var stream = new MemoryStream(rawMessage.Body))
                            {
                                command = consumer.serialiser.Deserialize(stream) as ICommand;
                            }
                            try
                            {
                                if (consumer.Handle(command, batchedUoW))
                                    endpoint.Acknowledge(rawMessage);
                            }
                            catch (Exception ex)
                            {
                                string error = String.Format("Error while handling command '{0}'", command.ToString());
                                log.Error(error, ex);
                            }
                        }


                    }

                }
                catch (EndpointClosedException ex)
                {
                    log.Error("Endpoint Closed", ex);

                }
                catch (Exception ex)
                {
                    log.Error("Unexpected Exception.", ex);
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