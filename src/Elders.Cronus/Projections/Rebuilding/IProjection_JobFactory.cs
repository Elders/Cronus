using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore.Players;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Projections.Rebuilding;

public interface IProjection_JobFactory
{
    ICronusJob<object> CreateJob(ProjectionVersion version, ReplayEventsOptions replayEventsOptions, VersionRequestTimebox timebox);
}
