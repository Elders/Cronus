using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index;

/// <summary>
/// Marker for event store indexes that are owned by the Cronus framework itself.
/// </summary>
public interface ICronusEventStoreIndex : IEventStoreIndex, ISystemHandler
{
}

/// <summary>
/// Builds and maintains an index over the event store's messages.
/// </summary>
public interface IEventStoreIndex : IMessageHandler
{
    /// <summary>
    /// Indexes the supplied <see cref="CronusMessage"/>.
    /// </summary>
    /// <param name="message">The Cronus message to index.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task IndexAsync(CronusMessage message, CancellationToken cancellationToken = default);
}
