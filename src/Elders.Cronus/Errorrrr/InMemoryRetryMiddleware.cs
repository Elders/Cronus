using System;
using Elders.Cronus.MessageProcessingMiddleware;
using Elders.Cronus.Middleware;
using Umbraco.Core.Persistence.FaultHandling.Strategies;

namespace Elders.Cronus.Errorrrr
{
    public class InMemoryRetryMiddleware : Middleware<HandleContext>
    {
        private Umbraco.Core.Persistence.FaultHandling.RetryPolicy retryPolicy;

        readonly Middleware<HandleContext> middleware;

        public InMemoryRetryMiddleware(Middleware<HandleContext> middleware)
        {
            this.middleware = middleware;
            var retryStrategy = new Incremental(3, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(100));//Total 3 Retrues
            retryPolicy = new Umbraco.Core.Persistence.FaultHandling.RetryPolicy(new Umbraco.Core.Persistence.FaultHandling.TransientErrorCatchAllStrategy(), retryStrategy);
        }
        protected override void Run(Execution<HandleContext> execution)
        {
            retryPolicy.ExecuteAction(() =>
            {
                middleware.Run(execution.Context);
            });
        }
    }
}
