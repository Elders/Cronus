using Elders.Cronus.FaultHandling;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.Pipeline.Config
{
    public static class RetryExtensions
    {
        public static Middleware<TContext> UseRetries<TContext>(this Middleware<TContext> self)
        {
            return new InMemoryRetryMiddleware<TContext>(self);
        }
    }
}