using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

public class PlayerOperator
{
    public Func<AggregateStream, Task> OnAggregateStreamLoadedAsync { get; set; }
    public Func<AggregateEventRaw, Task> OnLoadAsync { get; set; }
    public Func<PlayerOptions, Task> NotifyProgressAsync { get; set; }
    public Func<Task> OnFinish { get; set; }
}

public interface IEventStorePlayer<TSettings> : IEventStorePlayer where TSettings : class { }

public interface IEventStorePlayer
{
    /// <summary>
    /// Loads all aggregate commits. The commits are unordered.
    /// </summary>
    Task EnumerateEventStore(PlayerOperator @operator, PlayerOptions replayOptions, CancellationToken cancellationToken = default);
}
