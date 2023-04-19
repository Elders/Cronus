using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots.Strategy
{
    public interface ISnapshotStrategy<TContext>
    {
        Task<bool> ShouldCreateSnapshotAsync(SnapshotManagerId id, TContext context);
    }
}
