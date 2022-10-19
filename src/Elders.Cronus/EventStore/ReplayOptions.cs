using Elders.Cronus.EventStore.Index;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore
{
    public class ReplayOptions
    {
        public ReplayOptions()
        {
            AggregateIds = Enumerable.Empty<IAggregateRootId>();
            IndexRecords = Enumerable.Empty<IndexRecord>();
            ShouldSelect = commit => true;
        }

        [Obsolete("With the new EventStore structure we do not this anymore. Use IndexRecords collection instead.")]
        public IEnumerable<IAggregateRootId> AggregateIds { get; set; }

        public IEnumerable<IndexRecord> IndexRecords { get; set; }

        public string PaginationToken { get; set; }

        public Func<AggregateCommit, bool> ShouldSelect { get; set; }

        public int BatchSize { get; set; } = 1000;

        public DateTimeOffset? After { get; set; }

        public DateTimeOffset? Before { get; set; }
    }
}
