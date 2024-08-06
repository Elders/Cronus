using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.PartitionIndex
{
    public interface IProjectionPartionsStore
    {
        Task AppendAsync(ProjectionPartition record);

        IAsyncEnumerable<ProjectionPartition> GetPartitionsAsync(string projectionType, IBlobId projectionId);
    }

    public interface IInitializableProjectionPartionsStore
    {
        Task<bool> InitializeAsync();
    }
}
