using System;
using Elders.Cronus.Middleware;
using Umbraco.Core.Persistence.FaultHandling.Strategies;

namespace Elders.Cronus.Errorrrr
{
    public class InMemoryRetryMiddleware<TContext> : Middleware<TContext>
    {
        private Umbraco.Core.Persistence.FaultHandling.RetryPolicy retryPolicy;

        readonly Middleware<TContext> middleware;

        public InMemoryRetryMiddleware(Middleware<TContext> middleware)
        {
            this.middleware = middleware;
            var retryStrategy = new Incremental(3, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(100));//Total 3 Retrues
            retryPolicy = new Umbraco.Core.Persistence.FaultHandling.RetryPolicy(new Umbraco.Core.Persistence.FaultHandling.TransientErrorCatchAllStrategy(), retryStrategy);
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
