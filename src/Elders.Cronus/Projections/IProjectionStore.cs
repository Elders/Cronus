using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public interface IProjectionStore
    {
        IEnumerable<ProjectionCommit> Load(ProjectionVersion version, IBlobId projectionId, int snapshotMarker);

        Task<IEnumerable<ProjectionCommit>> LoadAsync(ProjectionVersion version, IBlobId projectionId, int snapshotMarker);

        IEnumerable<ProjectionCommit> EnumerateProjection(ProjectionVersion version, IBlobId projectionId);

        void InitializeProjectionStore(ProjectionVersion projectionVersion);

        void Save(ProjectionCommit commit);
    }
}
