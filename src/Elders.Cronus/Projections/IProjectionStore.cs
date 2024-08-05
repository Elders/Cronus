using Elders.Cronus.Projections.Cassandra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections;

public interface IProjectionStore
{
    IAsyncEnumerable<ProjectionCommit> LoadAsync(ProjectionVersion version, IBlobId projectionId, long partitionId);

    Task EnumerateProjectionsAsync(ProjectionsOperator @operator, ProjectionQueryOptions options);

    Task SaveAsync(ProjectionCommit commit);
}

public interface IInitializableProjectionStore
{
    Task<bool> InitializeAsync(ProjectionVersion version);
}
