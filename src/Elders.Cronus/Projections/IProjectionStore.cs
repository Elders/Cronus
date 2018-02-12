using System;
using System.Collections.Generic;

namespace Elders.Cronus.Projections
{
    public interface IProjectionStore
    {
        IEnumerable<ProjectionCommit> Load(ProjectionVersion version, IBlobId projectionId, int snapshotMarker);

        IEnumerable<ProjectionCommit> EnumerateProjection(ProjectionVersion version, IBlobId projectionId);

        void InitializeProjectionStore(ProjectionVersion projectionVersion);

        void Save(ProjectionCommit commit);
    }
}
