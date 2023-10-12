using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public interface IProjectionStore
    {
        IAsyncEnumerable<ProjectionCommitPreview> LoadAsync(ProjectionVersion version, IBlobId projectionId);

        Task SaveAsync(ProjectionCommitPreview commit);
    }

    public interface IInitializableProjectionStore
    {
        Task<bool> InitializeAsync(ProjectionVersion version);
    }
}
