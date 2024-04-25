using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Discoveries;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore.Players;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Rebuilding;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Cluster.Job.InMemory;

public class JobDiscovery : DiscoveryBase<ICronusJob<object>>
{
    protected override DiscoveryResult<ICronusJob<object>> DiscoverFromAssemblies(DiscoveryContext context)
    {
        return new DiscoveryResult<ICronusJob<object>>(GetModels(context).Concat(GetEventStoreJobs(context)));
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

        yield return new DiscoveredModel(typeof(IJobNameBuilder), typeof(DefaultJobNameBuilder), ServiceLifetime.Singleton);
        yield return new DiscoveredModel(typeof(DefaultJobNameBuilder), typeof(DefaultJobNameBuilder), ServiceLifetime.Singleton);
    }

    private IEnumerable<DiscoveredModel> GetEventStoreJobs(DiscoveryContext context)
    {
        var hasProjectionStore = context.FindService<IProjectionStore>().Any();
        bool hasEventStore = context.FindServiceExcept<IEventStore>(typeof(CronusEventStore)).Any();

        //if (hasEventStore)
        {
            yield return new DiscoveredModel(typeof(IRebuildIndex_EventToAggregateRootId_JobFactory), typeof(RebuildIndex_EventToAggregateRootId_JobFactory), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(RebuildIndex_MessageCounter_JobFactory), typeof(RebuildIndex_MessageCounter_JobFactory), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ReplayPublicEvents_JobFactory), typeof(ReplayPublicEvents_JobFactory), ServiceLifetime.Transient);

            //  if (hasProjectionStore)
            {
                yield return new DiscoveredModel(typeof(RebuildProjection_JobFactory), typeof(RebuildProjection_JobFactory), ServiceLifetime.Transient);
                yield return new DiscoveredModel(typeof(RebuildProjectionSequentially_JobFactory), typeof(RebuildProjectionSequentially_JobFactory), ServiceLifetime.Transient);
            }
        }
    }
}
