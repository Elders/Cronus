using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessing
{
    /// <summary>
    /// The responsibility of this middleware is to collect and and work with all the messsage subscirbers in the Cornus infrasturcutre.
    /// It also allows to dynamically add subscribers.
    /// Probably this class should be something like IObservable<ISubscriber>
    /// </summary>
    public interface ISubscriptionMiddleware<T>
    {
        IEnumerable<ISubscriber> Subscribers { get; }

        IEnumerable<ISubscriber> GetInterestedSubscribers(CronusMessage message);

        /// <summary>
        /// Adds a new subscriber to the Cornus infrastructure with intent to notify all intersted parties when a new subscriber comes in (e.g. for creating queus etc.)
        /// </summary>
        /// <param name="subscriber"></param>
        void Subscribe(ISubscriber subscriber);

        /// <summary>
        /// Removes all subscribers. Probably when shutting down.
        /// </summary>
        void UnsubscribeAll();
    }

    public class SubscriptionMiddleware<T> : ISubscriptionMiddleware<T>
    {
        ConcurrentBag<ISubscriber> subscribers;

        public SubscriptionMiddleware()
        {
            subscribers = new ConcurrentBag<ISubscriber>();
        }

        public void Subscribe(ISubscriber subscriber)
        {
            if (ReferenceEquals(null, subscriber)) throw new ArgumentNullException(nameof(subscriber));
            if (subscriber.GetInvolvedMessageTypes().Any() == false) throw new ArgumentException($"Subscirber '{subscriber.Id}' does not care about any message types. Any reason?");
            if (subscribers.Any(x => x.Id == subscriber.Id)) throw new ArgumentException($"There is already subscriber with id '{subscriber.Id}'");

            subscribers.Add(subscriber);
        }

        public IEnumerable<ISubscriber> GetInterestedSubscribers(CronusMessage message)
        {
            return Subscribers.Where(subscriber => subscriber.GetInvolvedMessageTypes().Contains(message.Payload.GetType()));
        }

        public IEnumerable<ISubscriber> Subscribers { get { return subscribers.ToList(); } }

        public void UnsubscribeAll()
        {
            subscribers = new ConcurrentBag<ISubscriber>();
        }
    }
}
