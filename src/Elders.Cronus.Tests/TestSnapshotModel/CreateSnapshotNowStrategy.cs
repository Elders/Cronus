using Elders.Cronus.Snapshots;
using Elders.Cronus.Snapshots.Strategy;
using System.Threading.Tasks;

public class CreateSnapshotNowStrategy : ISnapshotStrategy<AggregateSnapshotStrategyContext>
{
    public Task<bool> ShouldCreateSnapshotAsync(SnapshotManagerId id, AggregateSnapshotStrategyContext context)
    {
        return Task.FromResult(true);
    }
}
