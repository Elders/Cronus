using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.EventStore.Index
{
    public class DefaultJobNameBuilder : IJobNameBuilder
    {
        private readonly BoundedContext boundedContext;
        private readonly CronusContext context;

        public DefaultJobNameBuilder(IOptions<BoundedContext> boundedContext, CronusContext context)
        {
            this.boundedContext = boundedContext.Value;
            this.context = context;
        }

        public string GetJobName(string defaultName)
        {
            return $"urn:{boundedContext.Name}:{context.Tenant}:{defaultName}";
        }
    }
}
