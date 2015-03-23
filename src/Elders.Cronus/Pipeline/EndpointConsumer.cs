using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Serializer;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Pipeline.CircuitBreaker;

namespace Elders.Cronus.Pipeline
{
    public class EndpointConsumer : IEndpointConsumer
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumer));

        private readonly IMessageProcessor messageProcessor;

        private readonly IPipelineTransport transport;

        private readonly IEndpontCircuitBreakerFactrory circuitBreakerFactory;
        private readonly List<WorkPool> pools;
        private readonly ISerializer serializer;
        private readonly MessageThreshold messageThreshold;

        public int NumberOfWorkers { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointConsumer{TContract}"/> class.
        /// </summary>
        /// <param name="transport">The transport.</param>
        /// <param name="messageProcessor">The message processor.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageThreshold">The message threshold.</param>
        /// <param name="circuitBreakerFactory">The circuit breaker factory.</param>
        public EndpointConsumer(IPipelineTransport transport, IMessageProcessor messageProcessor, ISerializer serializer, MessageThreshold messageThreshold, IEndpontCircuitBreakerFactrory circuitBreakerFactory)
        {
            NumberOfWorkers = 1;
            this.messageProcessor = messageProcessor;
            this.transport = transport;
            pools = new List<WorkPool>();
            this.serializer = serializer;
            this.messageThreshold = messageThreshold;
            this.circuitBreakerFactory = circuitBreakerFactory;
        }

        public void Start(int? numberOfWorkers = null)
        {
            int workers = numberOfWorkers.HasValue
                ? numberOfWorkers.Value
                : NumberOfWorkers;

            pools.Clear();
            foreach (var endpointDefinition in transport.EndpointFactory.GetEndpointDefinition(messageProcessor))
            {
                var poolName = String.Format("Workpool {0}", endpointDefinition.EndpointName);
                WorkPool pool = new WorkPool(poolName, workers);
                for (int i = 0; i < workers; i++)
                {
                    IEndpoint endpoint = transport.EndpointFactory.CreateEndpoint(endpointDefinition);
                    pool.AddWork(new PipelineConsumerWork(messageProcessor, endpoint, serializer, messageThreshold, circuitBreakerFactory.Create(transport, serializer, endpointDefinition)));
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
