using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Multithreading.Work;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{
    public class EndpointConsumable<TContract> : IEndpointConsumable where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumable<TContract>));

        private readonly IConsumer<TContract> consumer;

        private readonly IEndpointFactory endpointFactory;

        private readonly List<WorkPool> pools;
        private readonly ProtoregSerializer serializer;
        private readonly int batchSize;

        public int NumberOfWorkers { get; set; }

        public EndpointConsumable(IEndpointFactory endpointFactory, IConsumer<TContract> consumer, ProtoregSerializer serializer, int batchSize)
        {
            NumberOfWorkers = 1;
            this.consumer = consumer;
            this.endpointFactory = endpointFactory;
            pools = new List<WorkPool>();
            this.serializer = serializer;
            this.batchSize = batchSize;
        }

        public void Start(int? numberOfWorkers = null)
        {
            int workers = numberOfWorkers.HasValue
                ? numberOfWorkers.Value
                : NumberOfWorkers;

            pools.Clear();
            var endpointDefinition = endpointFactory.GetEndpointDefinition(consumer.GetRegisteredHandlers.ToArray());

            var poolName = String.Format("Workpool {0}", endpointDefinition.EndpointName);
            WorkPool pool = new WorkPool(poolName, workers);
            for (int i = 0; i < workers; i++)
            {
                IEndpoint endpoint = endpointFactory.CreateEndpoint(endpointDefinition);
                if (consumer.PostConsume.ErrorStrategy != null)
                    consumer.PostConsume.ErrorStrategy.Initialize(endpointFactory, endpointDefinition);
                if (consumer.PostConsume.SuccessStrategy != null)
                    consumer.PostConsume.SuccessStrategy.Initialize(endpointFactory, endpointDefinition);
                pool.AddWork(new PipelineConsumerWork<TContract>(consumer, endpoint, serializer, batchSize));
            }
            pools.Add(pool);
            pool.StartCrawlers();
        }

        public void Stop()
        {
            pools.ForEach(pool => pool.Stop());
            pools.Clear();
        }

    }
}