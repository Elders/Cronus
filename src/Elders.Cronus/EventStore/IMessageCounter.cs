using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

/// <summary>
/// Tracks per-message-type cardinality counters maintained alongside the event store —
/// used by indices and dashboards to report event-volume statistics.
/// </summary>
public interface IMessageCounter
{
    /// <summary>
    /// Increments the counter for the supplied message type by <paramref name="incrementWith"/> (default 1).
    /// </summary>
    /// <param name="messageType">The message CLR type to increment the counter for.</param>
    /// <param name="incrementWith">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous increment operation.</returns>
    Task IncrementAsync(Type messageType, long incrementWith = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrements the counter for the supplied message type by <paramref name="decrementWith"/> (default 1).
    /// </summary>
    /// <param name="messageType">The message CLR type to decrement the counter for.</param>
    /// <param name="decrementWith">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous decrement operation.</returns>
    Task DecrementAsync(Type messageType, long decrementWith = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the counter for the supplied message type to zero.
    /// </summary>
    /// <param name="messageType">The message CLR type to reset.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous reset operation.</returns>
    Task ResetAsync(Type messageType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current count for the supplied message type.
    /// </summary>
    /// <param name="messageType">The message CLR type to query.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes with the current counter value.</returns>
    Task<long> GetCountAsync(Type messageType, CancellationToken cancellationToken = default);
}
