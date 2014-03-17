using System;
using System.Collections.Generic;
using System.Linq;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Multithreading.Work;

namespace NMSD.Cronus.Pipelining
{
    public class EndpointConsumable : IEndpointConsumable
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumable));

        private readonly IEndpointConsumer consumer;

        private readonly IEndpointFactory endpointFactory;

        private readonly List<WorkPool> pools;

        public EndpointConsumable(IEndpointFactory endpointFactory, IEndpointConsumer consumer)
        {
            this.consumer = consumer;
            this.endpointFactory = endpointFactory;
            pools = new List<WorkPool>();
        }

        public void Start(int numberOfWorkers)
        {
            pools.Clear();
            var endpointDefinitions = endpointFactory.GetEndpointDefinitions(consumer.GetRegisteredHandlers.ToArray());

            foreach (var endpointDefinition in endpointDefinitions)
            {
                var poolName = String.Format("Workpool {0}", endpointDefinition.EndpointName);
                WorkPool pool = new WorkPool(poolName, numberOfWorkers);
                for (int i = 0; i < numberOfWorkers; i++)
                {
                    pool.AddWork(new RabbitMqConsumerWork(consumer, endpointFactory.CreateEndpoint(endpointDefinition)));
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