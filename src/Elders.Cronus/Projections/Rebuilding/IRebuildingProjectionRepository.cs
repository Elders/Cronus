using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Projections.Rebuilding
{
    public interface IRebuildingProjectionRepository
    {
        public Task SaveAggregateCommitsAsync(IEnumerable<IndexRecord> indexRecords, RebuildProjection_JobData jobData);
    }

}
