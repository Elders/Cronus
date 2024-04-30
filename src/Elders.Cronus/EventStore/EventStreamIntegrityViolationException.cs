using System;

namespace Elders.Cronus.EventStore;

/// <summary>
///
/// </summary>
[Serializable]
public class EventStreamIntegrityViolationException : Exception
{
    /// <summary>
    /// Constructs a new EventStreamIntegrityViolationException.
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public EventStreamIntegrityViolationException(string message, Exception innerException)
        : base(message, innerException)
    { }

    /// <summary>
    /// Constructs a new EventStreamIntegrityViolationException.
    /// </summary>
    /// <param name="message">The exception message</param>
    public EventStreamIntegrityViolationException(string message) : base(message) { }

    /// <summary>
    /// Constructs a new EventStreamIntegrityViolationException.
    /// </summary>
    public EventStreamIntegrityViolationException() { }

}
