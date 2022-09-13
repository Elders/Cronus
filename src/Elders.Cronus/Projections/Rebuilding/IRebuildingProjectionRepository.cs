using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore;
using System;

namespace Elders.Cronus.Projections.Rebuilding
{
    public interface IRebuildingProjectionRepository
    {
        public Task<IEnumerable<EventStream>> LoadEventsAsync(IEnumerable<IndexRecord> indexRecords, ProjectionVersion version);
        public Task SaveAggregateCommitsAsync(IEnumerable<IndexRecord> eventStreams, RebuildProjection_JobData jobData);
    }

}
