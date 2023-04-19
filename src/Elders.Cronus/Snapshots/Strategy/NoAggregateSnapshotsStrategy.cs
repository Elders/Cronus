using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots.Strategy
{
    public sealed class NoAggregateSnapshotsStrategy : ISnapshotStrategy<AggregateSnapshotStrategyContext>
    {
        public Task<bool> ShouldCreateSnapshotAsync(SnapshotManagerId id, AggregateSnapshotStrategyContext context)
        {
            return Task.FromResult(false);
        }
    }
}
