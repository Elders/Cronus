using Elders.Cronus.Projections.Cassandra.EventSourcing;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index;

/// <summary>
/// Cronus event-store index that increments a counter for every event type that flows through it.
/// </summary>
[DataContract(Name = "f8c532eb-57ad-469f-9002-6c286bdd88f2")]
public class MessageCounterIndex : ICronusEventStoreIndex
{
    private readonly IMessageCounter eventCounter;

    public MessageCounterIndex(IMessageCounter eventCounter)
    {
        this.eventCounter = eventCounter;
    }

    /// <inheritdoc />
    public Task IndexAsync(CronusMessage message, CancellationToken cancellationToken = default)
    {
        if (message.Payload is IEvent @event)
        {
            return eventCounter.IncrementAsync(@event.Unwrap().GetType());
        }

        return Task.CompletedTask;
    }
}
