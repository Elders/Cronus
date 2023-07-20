using System.Threading;

namespace Elders.Cronus.MessageProcessing
{
    /// <summary>
    /// Provides an implementation of <see cref="IHttpContextAccessor" /> based on the current execution context.
    /// </summary>
    public class CronusContextAccessor : ICronusContextAccessor
    {
        private static readonly AsyncLocal<CronusContextHolder> _cronusContextCurrent = new AsyncLocal<CronusContextHolder>();

        /// <inheritdoc/>
        public CronusContext? CronusContext
        {
            get
            {
                return _cronusContextCurrent.Value?.Context;
            }
            set
            {
                var holder = _cronusContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _cronusContextCurrent.Value = new CronusContextHolder { Context = value };

                }
            }
        }

        private sealed class CronusContextHolder
        {
            public CronusContext? Context;
        }
    }
}
