using System;

namespace Elders.Cronus.EventStore
{
    public class PlayerOptions
    {
        private static DateTimeOffset MinAfterTimestamp = new DateTimeOffset(1602, 1, 1, 0, 0, 0, TimeSpan.Zero);   // 315360000000000
        private static DateTimeOffset MaxAfterTimestamp = DateTimeOffset.MaxValue.Subtract(TimeSpan.FromDays(100)); // 2650381343999999999

        public string EventTypeId { get; set; }

        public string PaginationToken { get; set; }

        public int BatchSize { get; set; } = 1000;

        public DateTimeOffset? After { get; set; } = MinAfterTimestamp;

        public DateTimeOffset? Before { get; set; } = MaxAfterTimestamp;

        public int MaxDegreeOfParallelism { get; set; } = 2;

        public PlayerOptions WithPaginationToken(string token)
        {
            return new PlayerOptions()
            {
                EventTypeId = this.EventTypeId,
                After = this.After,
                Before = this.Before,
                PaginationToken = token,
                BatchSize = this.BatchSize,
                MaxDegreeOfParallelism = this.MaxDegreeOfParallelism
            };
        }
    }
}
