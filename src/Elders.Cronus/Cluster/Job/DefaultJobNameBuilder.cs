using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.Cluster.Job;

public class DefaultJobNameBuilder : IJobNameBuilder
{
    private readonly BoundedContext boundedContext;
    private readonly ICronusContextAccessor contextAccessor;

    public DefaultJobNameBuilder(IOptions<BoundedContext> boundedContext, ICronusContextAccessor contextAccessor)
    {
        this.boundedContext = boundedContext.Value;
        this.contextAccessor = contextAccessor;
    }

    public string GetJobName(string defaultName)
    {
        return $"urn:{boundedContext.Name}:{contextAccessor.CronusContext.Tenant}:{defaultName}";
    }
}
