using Elders.Cronus.Projections.Cassandra;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections;

public interface IProjectionStore
{

    Task EnumerateProjectionsAsync(ProjectionsOperator @operator, ProjectionQueryOptions options);

    Task SaveAsync(ProjectionCommit commit);
}

public interface IInitializableProjectionStore
{
    Task<bool> InitializeAsync(ProjectionVersion version);
}
