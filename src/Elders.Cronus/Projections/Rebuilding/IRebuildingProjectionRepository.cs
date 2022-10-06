using System.Threading.Tasks;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Projections.Rebuilding
{
    public interface IRebuildingProjectionRepository
    {
        public Task SaveAggregateCommitsAsync(IndexRecord indexRecord, RebuildProjection_JobData jobData);
    }
}
