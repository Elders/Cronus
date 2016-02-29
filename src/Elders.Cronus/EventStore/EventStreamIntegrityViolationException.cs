using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;

namespace Elders.Cronus.EventStore
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EventStreamIntegrityViolationException : Exception
    {
        /// <summary>
        /// Constructs a new AggregateStateFirstLevelConcurrencyException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public EventStreamIntegrityViolationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Constructs a new AggregateStateFirstLevelConcurrencyException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public EventStreamIntegrityViolationException(string message) : base(message) { }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        protected EventStreamIntegrityViolationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Constructs a new AggregateStateFirstLevelConcurrencyException.
        /// </summary>
        public EventStreamIntegrityViolationException() { }

    }
}