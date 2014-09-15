using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Serializer;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Pipeline.CircuitBreaker;

namespace Elders.Cronus.Pipeline
{
    public class EndpointConsumer<TContract> : IEndpointConsumer, IDisposable where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumer<TContract>));

        private readonly IMessageProcessor<TContract> messageProcessor;

        private readonly IPipelineTransport transport;

        private readonly IEndpontCircuitBreakerFactrory circuitBreakerFactory;
        private readonly List<WorkPool> pools;
        private readonly ISerializer serializer;
        private readonly MessageThreshold messageThreshold;

        public int NumberOfWorkers { get; set; }

        public EndpointConsumer(IPipelineTransport transport, IMessageProcessor<TContract> messageProcessor, ISerializer serializer, MessageThreshold messageThreshold, IEndpontCircuitBreakerFactrory circuitBreakerFactory)
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
            var endpointDefinition = transport.EndpointFactory.GetEndpointDefinition(messageProcessor.GetRegisteredHandlers().ToArray());

            var poolName = String.Format("Workpool {0}", endpointDefinition.EndpointName);
            WorkPool pool = new WorkPool(poolName, workers);
            for (int i = 0; i < workers; i++)
            {
                IEndpoint endpoint = transport.EndpointFactory.CreateEndpoint(endpointDefinition);
                pool.AddWork(new PipelineConsumerWork<TContract>(messageProcessor, endpoint, serializer, messageThreshold, circuitBreakerFactory.Create(transport, serializer, endpointDefinition)));
            }
            pools.Add(pool);
            pool.StartCrawlers();
        }

        public void Stop()
        {
            pools.ForEach(pool => pool.Stop());
            pools.Clear();
        }


        public void Dispose()
        {
            transport.Dispose();
        }
    }
}