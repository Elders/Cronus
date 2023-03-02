using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots
{
    public interface ISnapshotStrategy
    {
        Task<bool> ShouldCreateSnapshotAsync(SnapshotManagerId id, int lastCompletedRevision, int newRevision);
    }
}
