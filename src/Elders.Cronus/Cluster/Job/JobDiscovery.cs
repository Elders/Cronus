using Elders.Cronus.Discoveries;
using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Elders.Cronus.Cluster.Job.InMemory
{
    public class JobDiscovery : DiscoveryBase<ICronusJob<object>>
    {
        protected override DiscoveryResult<ICronusJob<object>> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<ICronusJob<object>>(GetModels(context));
        }

        private IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            IEnumerable<System.Type> cronusJobs = context.FindService<ICronusJob<object>>();

            foreach (var job in cronusJobs)
            {
                yield return new DiscoveredModel(job, job, ServiceLifetime.Transient);
            }

            yield return new DiscoveredModel(typeof(TypeContainer<ICronusJob<object>>), new TypeContainer<ICronusJob<object>>(cronusJobs));

            yield return new DiscoveredModel(typeof(InMemoryCronusJobRunner), typeof(InMemoryCronusJobRunner), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ICronusJobRunner), typeof(InMemoryCronusJobRunner), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(RebuildIndex_EventToAggregateRootId_JobFactory), typeof(RebuildIndex_EventToAggregateRootId_JobFactory), ServiceLifetime.Transient);
        }
    }
}
