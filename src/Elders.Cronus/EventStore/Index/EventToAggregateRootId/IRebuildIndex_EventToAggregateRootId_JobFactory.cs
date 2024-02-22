using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.EventStore.Index;

public interface IRebuildIndex_EventToAggregateRootId_JobFactory
{
    ICronusJob<object> CreateJob(VersionRequestTimebox timebox, int maxDegreeOfParallelism);
    string GetJobName();
}
