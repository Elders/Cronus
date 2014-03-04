using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NMSD.Cronus.Transports;

namespace NMSD.Cronus.Transport.InMemory
{
    public class InMemoryQueue
    {
        private static InMemoryQueue current;

        static ConcurrentDictionary<IPipeline, ConcurrentDictionary<IEndpoint, BlockingCollection<EndpointMessage>>> pipelineStorage = new ConcurrentDictionary<IPipeline, ConcurrentDictionary<IEndpoint, BlockingCollection<EndpointMessage>>>(new PipelineComparer());

        public static InMemoryQueue Current
        {
            get
            {
                if (current == null)
                    current = new InMemoryQueue();
                return current;
            }
        }

        public void Bind(IPipeline pipeline, IEndpoint endpoint)
        {
            ConcurrentDictionary<IEndpoint, BlockingCollection<EndpointMessage>> endpointStorage;
            if (pipelineStorage.TryGetValue(pipeline, out endpointStorage))
            {
                if (endpointStorage.ContainsKey(endpoint))
                    return;
                endpointStorage.TryAdd(endpoint, new BlockingCollection<EndpointMessage>());
            }
        }

        public EndpointMessage BlockDequeue(IEndpoint endpoint)
        {
            BlockingCollection<EndpointMessage> endpointStorage;
            if (TryGetEndpointStorage(endpoint, out endpointStorage))
                return endpointStorage.Take();
            return null;
        }

        public bool BlockDequeue(IEndpoint endpoint, int timeoutInMiliseconds, out EndpointMessage msg)
        {
            msg = null;
            BlockingCollection<EndpointMessage> endpointStorage;
            if (TryGetEndpointStorage(endpoint, out endpointStorage))
                return endpointStorage.TryTake(out msg, timeoutInMiliseconds);
            return false;
        }

        public IEndpoint GetOrAddEndpoint(EndpointDefinition endpointDefinition)
        {
            var pipeline = GetOrAddPipeline(endpointDefinition.PipelineName);
            IEndpoint endpoint;
            if (!TryGetEndpoint(endpointDefinition.EndpointName, out endpoint))
            {
                endpoint = new InMemoryEndpoint(endpointDefinition.EndpointName, endpointDefinition.RoutingHeaders);
                Bind(pipeline, endpoint);
            }
            return endpoint;
        }

        public IPipeline GetOrAddPipeline(string pipelineName)
        {
            var pipeline = new InMemoryPipeline(pipelineName);
            if (!pipelineStorage.ContainsKey(pipeline))
            {
                pipelineStorage.TryAdd(pipeline, new ConcurrentDictionary<IEndpoint, BlockingCollection<EndpointMessage>>(new EndpointComparer()));
            }
            return pipeline;
        }

        public void SendMessage(IPipeline pipeline, EndpointMessage message)
        {
            ConcurrentDictionary<IEndpoint, BlockingCollection<EndpointMessage>> endpointStorage;
            if (pipelineStorage.TryGetValue(pipeline, out endpointStorage))
            {
                foreach (var store in endpointStorage)
                {
                    var endpoint = store.Key;

                    bool accept = false;
                    foreach (var messageHeader in message.Headers)
                    {
                        if (endpoint.RoutingHeaders.ContainsKey(messageHeader.Key))
                            accept = endpoint.RoutingHeaders[messageHeader.Key] == messageHeader.Value;
                        if (accept)
                            break;
                    }
                    if (!accept)
                        continue;

                    store.Value.TryAdd(message);
                }
            }
        }

        private bool TryGetEndpoint(string endpointName, out IEndpoint endpoint)
        {
            endpoint = null;
            var searchResult = (from pipeline in pipelineStorage
                                from es in pipelineStorage.Values
                                where es.Keys.Any(ep => ep.Name == endpointName)
                                select es)
                                .SingleOrDefault();
            if (searchResult != null)
                endpoint = searchResult.First().Key;

            return !ReferenceEquals(null, endpoint);
        }

        private bool TryGetEndpointStorage(IEndpoint endpoint, out BlockingCollection<EndpointMessage> endpointStorage)
        {
            endpointStorage = null;
            var searchResult = (from es in pipelineStorage.Values
                                where es.Keys.Contains(endpoint)
                                select es)
                                .SingleOrDefault();
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

    }
}
