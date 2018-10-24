using Elders.Cronus.FaultHandling;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.Pipeline.Config
{
    public static class RetryExtensions
    {
        public static Workflow<TContext> UseRetries<TContext>(this Workflow<TContext> self)
        {
            return new InMemoryRetryWorkflow<TContext>(self);
        }
    }
}
