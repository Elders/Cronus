using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.Projections.Cassandra;

namespace Elders.Cronus.Projections;

internal sealed class MissingProjections : IProjectionStore, IInitializableProjectionStore
{
    private const string MissingProjectionsMessage = "The Projections feature is not installed or properly configured. Please install a nuget package which provides IProjectionStore capabilities. ex.: Cronus.Projections.Cassandra. You can disable the projections functionality with Cronus:ProjectionsEnabled = false";

    public Task EnumerateProjectionsAsync(ProjectionsOperator @operator, ProjectionQueryOptions options)
    {
        throw new NotImplementedException(MissingProjectionsMessage);
    }

    public Task<bool> InitializeAsync(ProjectionVersion version)
    {
        throw new NotImplementedException(MissingProjectionsMessage);
    }

    public Task SaveAsync(ProjectionCommit commit)
    {
        throw new NotImplementedException(MissingProjectionsMessage);
    }
}
