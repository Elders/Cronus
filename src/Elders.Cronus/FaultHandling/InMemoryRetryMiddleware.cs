using System;
using Elders.Cronus.FaultHandling.Strategies;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.FaultHandling
{
    public class InMemoryRetryMiddleware<TContext> : Middleware<TContext>
    {
        private RetryPolicy retryPolicy;

        readonly Middleware<TContext> middleware;

        public InMemoryRetryMiddleware(Middleware<TContext> middleware)
        {
            this.middleware = middleware;
            var retryStrategy = new Incremental(3, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(100));//Total 3 Retrues
            retryPolicy = new RetryPolicy(new TransientErrorCatchAllStrategy(), retryStrategy);
        }
        protected override void Run(Execution<TContext> execution)
        {
            retryPolicy.ExecuteAction(() =>
            {
                middleware.Run(execution.Context);
            });
        }
    }
}
