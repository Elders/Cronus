using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.AtomicAction
{
    [Serializable]
    public class AggregateStateFirstLevelConcurrencyException : Exception
    {
        /// <summary>
        /// Constructs a new AggregateStateFirstLevelConcurrencyException.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public AggregateStateFirstLevelConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Constructs a new AggregateStateFirstLevelConcurrencyException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public AggregateStateFirstLevelConcurrencyException(string message) : base(message) { }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        protected AggregateStateFirstLevelConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Constructs a new AggregateStateFirstLevelConcurrencyException.
        /// </summary>
        public AggregateStateFirstLevelConcurrencyException() { }
    }
}
