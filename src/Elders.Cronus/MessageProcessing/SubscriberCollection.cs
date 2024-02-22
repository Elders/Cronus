using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessing;

/// <summary>
/// The responsibility of this collection is to collect and work with all the message subscribers in the Cronus infrastructure.
/// It also allows to dynamically add subscribers.
/// </summary>
public sealed class SubscriberCollection<T> : ISubscriberCollection<T>
{
    ConcurrentBag<ISubscriber> subscribers;
    private readonly ILogger<SubscriberCollection<T>> logger;

    public SubscriberCollection(ISubscriberFinder<T> subscriberFinder, ISubscriberFactory<T> subscriberFactory, ILogger<SubscriberCollection<T>> logger)
    {
        subscribers = new ConcurrentBag<ISubscriber>();

        foreach (var subscriberType in subscriberFinder.Find())
        {
            ISubscriber subscriber = subscriberFactory.Create(subscriberType);
            Subscribe(subscriber);
        }

        this.logger = logger;
    }

    /// <summary>
    /// Adds a new subscriber to the Cronus infrastructure with intent to notify all interested parties when a new subscriber comes in (e.g. for creating queues etc.)
    /// </summary>
    /// <param name="subscriber">The subscriber.</param>
    public void Subscribe(ISubscriber subscriber)
    {
        if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
        if (subscribers.Any(x => x.Id == subscriber.Id)) throw new ArgumentException($"There is already subscriber with id '{subscriber.Id}'");

        subscribers.Add(subscriber);
    }

    public IEnumerable<ISubscriber> Subscribers { get { return subscribers; } }

    public IEnumerable<ISubscriber> GetInterestedSubscribers(CronusMessage message)
    {
        try
        {
            IEnumerable<ISubscriber> result = Subscribers;

            Type payloadType = message.GetMessageType();

            if (message.RecipientHandlers.Length > 0)
            {
                result = result.Where(subscriber => message.RecipientHandlers.Contains(subscriber.Id));
            }

            result = result.Where(subscriber => subscriber.GetInvolvedMessageTypes().Contains(payloadType));

            return result;
        }
        catch (Exception ex) when (logger.ErrorException(ex, () => "An error occured while getting subscribers for message."))
        {
            return Enumerable.Empty<ISubscriber>();
        }

    }
    /// <summary>
    /// Removes all subscribers. Probably when shutting down.
    /// </summary>
    public void UnsubscribeAll()
    {
        subscribers = new ConcurrentBag<ISubscriber>();
    }
}
