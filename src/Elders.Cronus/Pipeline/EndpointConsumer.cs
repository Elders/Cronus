using System;
using System.Collections.Generic;
using Elders.Cronus.Serializer;
using Elders.Cronus.Pipeline.Transport;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline
{
    public class EndpointConsumer : IEndpointConsumer
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(EndpointConsumer));

        readonly SubscriptionMiddleware subscriptions;
        readonly IPipelineTransport transport;
        readonly List<WorkPool> pools;
        readonly ISerializer serializer;
        readonly MessageThreshold messageThreshold;

        public string Name { get; private set; }

        public int NumberOfWorkers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointConsumer"/> class.
        /// </summary>
        /// <param name="transport">The transport.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageThreshold">The message threshold.</param>
        public EndpointConsumer(string name, IPipelineTransport transport, SubscriptionMiddleware subscriptions, ISerializer serializer, MessageThreshold messageThreshold)
        {
            this.Name = name;
            NumberOfWorkers = 1;
            this.subscriptions = subscriptions;
            this.transport = transport;
            pools = new List<WorkPool>();
            this.serializer = serializer;
            this.messageThreshold = messageThreshold;

        }

        public void Start(int? numberOfWorkers = null)
        {
            int workers = numberOfWorkers.HasValue
                ? numberOfWorkers.Value
                : NumberOfWorkers;

            pools.Clear();
            foreach (var endpointDefinition in transport.EndpointFactory.GetEndpointDefinition(this, subscriptions))
            {
                var poolName = String.Format("Workpool {0}", endpointDefinition.EndpointName);
                WorkPool pool = new WorkPool(poolName, workers);
                for (int i = 0; i < workers; i++)
                {
                    IEndpoint endpoint = transport.EndpointFactory.CreateEndpoint(endpointDefinition);
                    pool.AddWork(new PipelineConsumerWork(subscriptions, endpoint, serializer, messageThreshold));
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
