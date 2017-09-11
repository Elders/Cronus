using System;
using System.Collections.Generic;
using Elders.Cronus.Serializer;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Cronus.Pipeline
{
    public class EndpointConsumer : IEndpointConsumer
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(EndpointConsumer));

        readonly SubscriptionMiddleware subscriptions;
        readonly IPipelineTransport transport;
        readonly ISerializer serializer;
        readonly List<IEndpoint> startedEndpoints;

        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointConsumer"/> class.
        /// </summary>
        /// <param name="transport">The transport.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageThreshold">The message threshold.</param>
        public EndpointConsumer(string name, IPipelineTransport transport, SubscriptionMiddleware subscriptions, ISerializer serializer)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Invalid consumer name", nameof(name));
            if (ReferenceEquals(null, transport)) throw new ArgumentNullException(nameof(transport));
            if (ReferenceEquals(null, subscriptions)) throw new ArgumentNullException(nameof(subscriptions));
            if (subscriptions.Subscribers.Count() == 0) throw new ArgumentException("A consumer must have at least one subscriber to work properly.", nameof(subscriptions));
            if (ReferenceEquals(null, serializer)) throw new ArgumentNullException(nameof(serializer));

            this.Name = name;
            this.subscriptions = subscriptions;
            this.transport = transport;
            this.serializer = serializer;
            this.startedEndpoints = new List<IEndpoint>();
        }

        public void Start()
        {
            foreach (var endpointDefinition in transport.EndpointFactory.GetEndpointDefinition(this, subscriptions))
            {
                IEndpoint endpoint = transport.EndpointFactory.CreateEndpoint(endpointDefinition);

                endpoint.OnMessage((msg) =>
                {
                    var subscribers = subscriptions.GetInterestedSubscribers(msg);
                    foreach (var subscriber in subscribers)
                    {
                        subscriber.Process(msg);
                    }
                });

                endpoint.Start();
                startedEndpoints.Add(endpoint);
            }
        }

        public void Stop()
        {
            Parallel.ForEach(startedEndpoints, x => x.Stop());
            startedEndpoints.Clear();
        }
    }
}
