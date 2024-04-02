using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore;

/// <summary>
///
/// </summary>
[Serializable]
public class AggregateLoadException : Exception
{
    /// <summary>
    /// Constructs a new AggregateLoadException.
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public AggregateLoadException(string message, Exception innerException)
        : base(message, innerException)
    { }

    /// <summary>
    /// Constructs a new AggregateLoadException.
    /// </summary>
    /// <param name="message">The exception message</param>
    public AggregateLoadException(string message) : base(message) { }

    /// <summary>
    /// Constructs a new AggregateStateFirstLevelConcurrencyException.
    /// </summary>
    public AggregateLoadException() { }

}
