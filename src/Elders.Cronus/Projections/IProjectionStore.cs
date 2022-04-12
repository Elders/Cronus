using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public interface IProjectionStore
    {
        Task<IEnumerable<ProjectionCommit>> LoadAsync(ProjectionVersion version, IBlobId projectionId, int snapshotMarker);

        bool HasSnapshotMarker(ProjectionVersion version, IBlobId projectionId, int snapshotMarker);

        IEnumerable<ProjectionCommit> EnumerateProjection(ProjectionVersion version, IBlobId projectionId);

        Task SaveAsync(ProjectionCommit commit);
    }

    public interface IInitializableProjectionStore
    {
        Task InitializeAsync(ProjectionVersion version);
    }

    public class NotInitializableProjectionStore : IInitializableProjectionStore
    {
        public Task InitializeAsync(ProjectionVersion version) { return Task.CompletedTask; }
    }
}
