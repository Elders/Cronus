using Elders.Cronus.EventStore.Index;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore
{
    public class ReplayOptions
    {
        private static DateTimeOffset MinAfterTimestamp = new DateTimeOffset(1602, 1, 1, 0, 0, 0, TimeSpan.Zero);   // 315360000000000
        private static DateTimeOffset MaxAfterTimestamp = DateTimeOffset.MaxValue.Subtract(TimeSpan.FromDays(100)); // 2650381343999999999

        public ReplayOptions()
        {
            ShouldSelect = commit => true;
        }

        [Obsolete("With the new EventStore structure we do not this anymore. Use IndexRecords collection instead.")]
        public IEnumerable<AggregateRootId> AggregateIds { get; set; }

        [Obsolete("With the new EventStore structure we do not this anymore. Use IndexRecords collection instead.")]
        public IEnumerable<IndexRecord> IndexRecords { get; set; }

        public string EventTypeId { get; set; }

        public string PaginationToken { get; set; }

        public Func<AggregateCommit, bool> ShouldSelect { get; set; }

        public int BatchSize { get; set; } = 1000;

        public DateTimeOffset? After { get; set; } = MinAfterTimestamp;

        public DateTimeOffset? Before { get; set; } = MaxAfterTimestamp;

        public ReplayOptions WithPaginationToken(string token)
        {
            return new ReplayOptions()
            {
                EventTypeId = this.EventTypeId,
                After = this.After,
                Before = this.Before,
                PaginationToken = token,
                BatchSize = this.BatchSize,
                ShouldSelect = this.ShouldSelect
            };
        }
    }
}
