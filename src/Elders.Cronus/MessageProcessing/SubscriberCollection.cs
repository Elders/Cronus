using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessing
{
    /// <summary>
    /// The responsibility of this collection is to collect and work with all the message subscribers in the Cronus infrastructure.
    /// It also allows to dynamically add subscribers.
    /// </summary>
    public sealed class SubscriberCollection<T> : ISubscriberCollection<T>
    {
        ConcurrentBag<ISubscriber> subscribers;
        private readonly ISubscriberFinder<T> subscriberFinder;
        private readonly ISubscriberFactory<T> subscriberFactory;

        public SubscriberCollection(ISubscriberFinder<T> subscriberFinder, ISubscriberFactory<T> subscriberFactory)
        {
            subscribers = new ConcurrentBag<ISubscriber>();

            this.subscriberFinder = subscriberFinder;
            this.subscriberFactory = subscriberFactory;
            foreach (var handlerType in subscriberFinder.Find())
            {
                ISubscriber subscriber = subscriberFactory.Create(handlerType);
                Subscribe(subscriber);
            }
        }

        /// <summary>
        /// Adds a new subscriber to the Cronus infrastructure with intent to notify all interested parties when a new subscriber comes in (e.g. for creating queues etc.)
        /// </summary>
        /// <param name="subscriber"></param>
        public void Subscribe(ISubscriber subscriber)
        {
            if (ReferenceEquals(null, subscriber)) throw new ArgumentNullException(nameof(subscriber));
            if (subscribers.Any(x => x.Id == subscriber.Id)) throw new ArgumentException($"There is already subscriber with id '{subscriber.Id}'");

            subscribers.Add(subscriber);
        }

        public IEnumerable<ISubscriber> Subscribers { get { return subscribers; } }

        public IEnumerable<ISubscriber> GetInterestedSubscribers(CronusMessage message)
        {
            return Subscribers.Where(subscriber => subscriber.GetInvolvedMessageTypes().Contains(message.Payload.GetType()));
        }

        /// <summary>
        /// Removes all subscribers. Probably when shutting down.
        /// </summary>
        public void UnsubscribeAll()
        {
            subscribers = new ConcurrentBag<ISubscriber>();
        }
    }
}
