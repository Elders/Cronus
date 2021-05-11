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
            ShouldSelect = commit => true;
        }

        public IEnumerable<IAggregateRootId> AggregateIds { get; set; }

        public string PaginationToken { get; set; }

        public Func<AggregateCommit, bool> ShouldSelect { get; set; }

        public int BatchSize { get; set; } = 1000;

        public DateTimeOffset? After { get; set; }

        public DateTimeOffset? Before { get; set; }
    }
}
