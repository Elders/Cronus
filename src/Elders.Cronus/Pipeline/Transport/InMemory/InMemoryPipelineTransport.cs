using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Transport.InMemory
{
    public class InMemoryPipelineTransport : IPipelineTransport
    {
        public static int TotalMessagesConsumed { get; private set; }

        static ConcurrentDictionary<IPipeline, ConcurrentDictionary<IEndpoint, BlockingCollection<CronusMessage>>> pipelineStorage = new ConcurrentDictionary<IPipeline, ConcurrentDictionary<IEndpoint, BlockingCollection<CronusMessage>>>(new PipelineComparer());

        public void Bind(IPipeline pipeline, IEndpoint endpoint)
        {
            ConcurrentDictionary<IEndpoint, BlockingCollection<CronusMessage>> endpointStorage;
            if (pipelineStorage.TryGetValue(pipeline, out endpointStorage))
            {
                if (endpointStorage.ContainsKey(endpoint))
                    return;
                endpointStorage.TryAdd(endpoint, new BlockingCollection<CronusMessage>());
            }
        }

        public bool BlockDequeue(IEndpoint endpoint, uint timeoutInMiliseconds, out CronusMessage msg)
        {
            msg = null;
            BlockingCollection<CronusMessage> endpointStorage;
            if (TryGetEndpointStorage(endpoint, out endpointStorage))
            {

                if (endpointStorage.TryTake(out msg, (int)timeoutInMiliseconds))
                {
                    TotalMessagesConsumed++;
                    return true;
                }
                return false;
            }
            return false;
        }

        public IEndpoint GetOrAddEndpoint(EndpointDefinition endpointDefinition)
        {
            var pipeline = GetOrAddPipeline(endpointDefinition.PipelineName);
            IEndpoint endpoint;
            if (!TryGetEndpoint(endpointDefinition.EndpointName, out endpoint))
            {
                endpoint = new InMemoryEndpoint(this, endpointDefinition.EndpointName, endpointDefinition.WatchMessageTypes);
                Bind(pipeline, endpoint);
            }
            return endpoint;
        }

        public IPipeline GetOrAddPipeline(string pipelineName)
        {
            var pipeline = new InMemoryPipeline(this, pipelineName);
            if (!pipelineStorage.ContainsKey(pipeline))
            {
                pipelineStorage.TryAdd(pipeline, new ConcurrentDictionary<IEndpoint, BlockingCollection<CronusMessage>>(new EndpointComparer()));
            }
            return pipeline;
        }

        public void SendMessage(IPipeline pipeline, CronusMessage message)
        {
            ConcurrentDictionary<IEndpoint, BlockingCollection<CronusMessage>> endpointStorage;
            if (pipelineStorage.TryGetValue(pipeline, out endpointStorage))
            {
                var messageType = message.Payload.GetType().GetContractId();

                foreach (var store in endpointStorage)
                {
                    var endpoint = store.Key as InMemoryEndpoint;

                    if (endpoint == null || !endpoint.WatchMessageTypes.Contains(messageType))
                    {
                        continue;
                    }

                    store.Value.TryAdd(message);
                }
            }
        }

        private bool TryGetEndpoint(string endpointName, out IEndpoint endpoint)
        {
            endpoint = null;
            var searchResult = pipelineStorage.Values
                .SingleOrDefault(es => es.Keys.Any(ep => ep.Name == endpointName));

            if (searchResult != null)
                endpoint = searchResult.First().Key;

            return !ReferenceEquals(null, endpoint);
        }

        private bool TryGetEndpointStorage(IEndpoint endpoint, out BlockingCollection<CronusMessage> endpointStorage)
        {
            endpointStorage = null;
            var searchResult = pipelineStorage.Values
                .SingleOrDefault(es => es.Keys.Contains(endpoint));

            if (searchResult != null)
                endpointStorage = searchResult[endpoint];

            return !ReferenceEquals(null, endpointStorage);
        }

        public class PipelineComparer : IEqualityComparer<IPipeline>
        {
            public bool Equals(IPipeline x, IPipeline y)
            {
                return x.Name.Equals(y.Name);
            }

            public int GetHashCode(IPipeline obj)
            {
                return 133 ^ obj.Name.GetHashCode();
            }
        }

        public class EndpointComparer : IEqualityComparer<IEndpoint>
        {
            public bool Equals(IEndpoint x, IEndpoint y)
            {
                return x.Name.Equals(y.Name);
            }

            public int GetHashCode(IEndpoint obj)
            {
                return 101 ^ obj.Name.GetHashCode();
            }
        }

        public InMemoryPipelineTransport(IPipelineNameConvention pipelineNameConvention, IEndpointNameConvention endpointNameConvention)
        {
            this.EndpointFactory = new InMemoryEndpointFactory(this, endpointNameConvention);
            this.PipelineFactory = new InMemoryPipelineFactory(this, pipelineNameConvention);
        }

        public IEndpointFactory EndpointFactory { get; private set; }

        public IPipelineFactory<IPipeline> PipelineFactory { get; private set; }

        public void Dispose()
        {
        }
    }
}
