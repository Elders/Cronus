using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Serializer;
using Elders.Cronus.Pipeline.Transport;

namespace Elders.Cronus.Pipeline
{
    public class EndpointConsumerHost<TContract> : IEndpointConsumerHost, IDisposable where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumerHost<TContract>));

        private readonly IConsumer<TContract> consumer;

        private readonly IPipelineTransport transport;

        private readonly List<WorkPool> pools;
        private readonly ISerializer serializer;
        private readonly MessageThreshold messageThreshold;

        public int NumberOfWorkers { get; set; }

        public EndpointConsumerHost(IPipelineTransport transport, IConsumer<TContract> consumer, ISerializer serializer, MessageThreshold messageThreshold)
        {
            NumberOfWorkers = 1;
            this.consumer = consumer;
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
            var endpointDefinition = transport.EndpointFactory.GetEndpointDefinition(consumer.GetRegisteredHandlers.ToArray());

            var poolName = String.Format("Workpool {0}", endpointDefinition.EndpointName);
            WorkPool pool = new WorkPool(poolName, workers);
            for (int i = 0; i < workers; i++)
            {
                IEndpoint endpoint = transport.EndpointFactory.CreateEndpoint(endpointDefinition);
                if (consumer.PostConsume.ErrorStrategy != null)
                    consumer.PostConsume.ErrorStrategy.Initialize(transport.EndpointFactory, endpointDefinition);
                if (consumer.PostConsume.RetryStrategy != null)
                    consumer.PostConsume.RetryStrategy.Initialize(transport.EndpointFactory, endpointDefinition);
                if (consumer.PostConsume.SuccessStrategy != null)
                    consumer.PostConsume.SuccessStrategy.Initialize(transport.EndpointFactory, endpointDefinition);
                pool.AddWork(new PipelineConsumerWork<TContract>(consumer, endpoint, serializer, messageThreshold));
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