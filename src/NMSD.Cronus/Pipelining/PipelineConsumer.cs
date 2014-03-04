using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Multithreading.Work;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Pipelining
{
    public class PipelineConsumer<T> : IStartableConsumer<T> where T : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PipelineConsumer<T>));

        private readonly IEndpointFactory endpointFactory;

        private readonly MessageHandlerCollection<T> messageHandlers;

        private readonly List<WorkPool> pools;

        private readonly ProtoregSerializer serialiser;
        public ScopeFactory ScopeFactory { get; set; }
        public PipelineConsumer(IEndpointFactory endpointFactory, ProtoregSerializer serialiser, MessageHandlerCollection<T> messageHandlers, ScopeFactory scopeFactory)
        {
            this.messageHandlers = messageHandlers;
            this.endpointFactory = endpointFactory;
            this.serialiser = serialiser;
            ScopeFactory = scopeFactory;
            messageHandlers.ScopeFactory = scopeFactory;
            pools = new List<WorkPool>();
        }

        public bool Consume(IEndpoint endpoint)
        {
            var context = new Context();
            return ScopeFactory.UseBatchScope(batch =>
            {
                for (int i = 0; i < batch.Size; i++)
                {
                    var rawMessage = endpoint.BlockDequeue(batch);

                    T message;
                    using (var stream = new MemoryStream(rawMessage.Body))
                    {
                        message = (T)serialiser.Deserialize(stream);
                    }

                    try
                    {
                        if (messageHandlers.Handle(message))
                            endpoint.Acknowledge(rawMessage);
                    }
                    catch (Exception ex)
                    {
                        string error = String.Format("Error while handling message '{0}'", message.ToString());
                        log.Error(error, ex);
                    }
                }
                return true;
            });
        }



        public void Start(int numberOfWorkers)
        {
            pools.Clear();
            var endpointDefinitions = endpointFactory.GetEndpointDefinitions(messageHandlers.GetRegisteredHandlers().ToArray());

            foreach (var endpointDefinition in endpointDefinitions)
            {
                var poolName = String.Format("Workpool {0}", endpointDefinition.EndpointName);
                WorkPool pool = new WorkPool(poolName, numberOfWorkers);
                for (int i = 0; i < numberOfWorkers; i++)
                {
                    pool.AddWork(new RabbitMqConsumerWork(this, endpointFactory.CreateEndpoint(endpointDefinition)));
                }
                pools.Add(pool);
                pool.StartCrawlers();
            }
        }

        public void Stop()
        {
            pools.ForEach(pool => pool.Stop());
            pools.Clear();
        }

    }
}
