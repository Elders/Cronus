using System;

namespace Elders.Cronus.EventStore;

public class PlayerOptions
{
    private static DateTimeOffset MinAfterTimestamp = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static DateTimeOffset MaxAfterTimestamp = DateTimeOffset.MaxValue.Subtract(TimeSpan.FromDays(100)); // 2650381343999999999

    public string EventTypeId { get; set; }

    public string PaginationToken { get; set; }

    public int BatchSize { get; set; } = 1000;
    public int Loaded { get; set; }

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

    public PlayerOptions WithLoaded(int loaded)
    {
        return new PlayerOptions()
        {
            EventTypeId = this.EventTypeId,
            After = this.After,
            Before = this.Before,
            PaginationToken = this.PaginationToken,
            BatchSize = this.BatchSize,
            MaxDegreeOfParallelism = this.MaxDegreeOfParallelism,
            Loaded = loaded
        };
    }
}
