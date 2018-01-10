using System;
using System.Collections.Generic;
using Elders.Cronus.Serializer;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using System.Linq;
using System.Threading.Tasks;
using Elders.Multithreading.Scheduler;

namespace Elders.Cronus.Pipeline
{
    public class CronusConsumer : ICronusConsumer
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(CronusConsumer));

        readonly SubscriptionMiddleware subscriptions;
        readonly ITransport transport;
        readonly List<WorkPool> pools;
        readonly ISerializer serializer;

        public string Name { get; private set; }
        public int NumberOfWorkers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CronusConsumer"/> class.
        /// </summary>
        /// <param name="transport">The transport.</param>
        /// <param name="serializer">The serializer.</param>
        public CronusConsumer(string name, ITransport transport, SubscriptionMiddleware subscriptions, ISerializer serializer)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Invalid consumer name", nameof(name));
            if (ReferenceEquals(null, transport)) throw new ArgumentNullException(nameof(transport));
            if (ReferenceEquals(null, subscriptions)) throw new ArgumentNullException(nameof(subscriptions));
            if (subscriptions.Subscribers.Count() == 0) throw new ArgumentException("A consumer must have at least one subscriber to work properly.", nameof(subscriptions));
            if (ReferenceEquals(null, serializer)) throw new ArgumentNullException(nameof(serializer));

            this.Name = name;
            NumberOfWorkers = 1;
            this.subscriptions = subscriptions;
            this.transport = transport;
            this.serializer = serializer;
            pools = new List<WorkPool>();
        }

        public void Start(int? numberOfWorkers = null)
        {
            int workers = numberOfWorkers.HasValue ? numberOfWorkers.Value : NumberOfWorkers;

            pools.Clear();

            foreach (var factory in transport.GetAvailableConsumers(serializer, subscriptions, Name))
            {
                var poolName = string.Format("cronus: " + Name);
                WorkPool pool = new WorkPool(poolName, workers);
                for (int i = 0; i < workers; i++)
                {
                    pool.AddWork(factory.CreateConsumer());
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
