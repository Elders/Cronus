using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

/// <summary>
/// Set of asynchronous callbacks the event-store player invokes while replaying events.
/// </summary>
public class PlayerOperator
{
    /// <summary>
    /// Invoked when a complete aggregate stream has been loaded.
    /// </summary>
    public Func<AggregateStream, CancellationToken, Task> OnAggregateStreamLoadedAsync { get; set; }

    /// <summary>
    /// Invoked for each raw aggregate event the player encounters.
    /// </summary>
    public Func<AggregateEventRaw, CancellationToken, Task> OnLoadAsync { get; set; }

    /// <summary>
    /// Invoked periodically so callers can publish progress notifications.
    /// </summary>
    public Func<PlayerOptions, CancellationToken, Task> NotifyProgressAsync { get; set; }

    /// <summary>
    /// Invoked once the player has finished replaying events.
    /// </summary>
    public Func<CancellationToken, Task> OnFinish { get; set; }
}

public interface IEventStorePlayer<TSettings> : IEventStorePlayer where TSettings : class { }

public interface IEventStorePlayer
{
    /// <summary>
    /// Loads all aggregate commits. The commits are unordered.
    /// </summary>
    Task EnumerateEventStore(PlayerOperator @operator, PlayerOptions replayOptions, CancellationToken cancellationToken = default);
}
