﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public interface IProjectionStore
    {
        IAsyncEnumerable<ProjectionCommit> LoadAsync(ProjectionVersion version, IBlobId projectionId, int snapshotMarker);

        Task<bool> HasSnapshotMarkerAsync(ProjectionVersion version, IBlobId projectionId, int snapshotMarker);

        IAsyncEnumerable<ProjectionCommit> EnumerateProjectionAsync(ProjectionVersion version, IBlobId projectionId);

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
