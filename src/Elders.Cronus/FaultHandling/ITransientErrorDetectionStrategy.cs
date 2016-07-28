using System;

namespace Elders.Cronus.FaultHandling
{
    /// <summary>
    /// Defines an interface which must be implemented by custom components responsible for detecting specific transient conditions.
    /// </summary>
    public interface ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>True if the specified exception is considered as transient, otherwise false.</returns>
        bool IsTransient(Exception ex);
    }
    /// <summary>
    /// Implements a strategy that ignores any transient errors.
    /// </summary>
    public sealed class TransientErrorIgnoreStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Always return false.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>Returns false.</returns>
        public bool IsTransient(Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Implements a strategy that treats all exceptions as transient errors.
    /// </summary>
    public sealed class TransientErrorCatchAllStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Always return true.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>Returns true.</returns>
        public bool IsTransient(Exception ex)
        {
            return true;
        }
    }
}
