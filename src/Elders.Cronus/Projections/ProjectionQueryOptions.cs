using Elders.Cronus.EventStore;
using System;

namespace Elders.Cronus.Projections.Cassandra;

public readonly struct ProjectionQueryOptions
{
    private const int DefaultBatchSize = 1000;

    ProjectionQueryOptions(IBlobId id, ProjectionVersion version, PagingOptions pagingOptions, DateTimeOffset? asOf, int? batchSize)
    {
        Id = id;
        Version = version;
        PagingOptions = pagingOptions;
        AsOf = asOf;
        batchSize ??= DefaultBatchSize;
        BatchSize = batchSize.Value;
    }

    public ProjectionQueryOptions(IBlobId id, ProjectionVersion version, DateTimeOffset asOf) : this(id, version, default, asOf, DefaultBatchSize) { }

    public ProjectionQueryOptions(IBlobId id, ProjectionVersion version, PagingOptions options) : this(id, version, options, null, options.Take) { }

    public ProjectionQueryOptions(IBlobId id, ProjectionVersion version) : this(id, version, null, null, DefaultBatchSize) { }

    public IBlobId Id { get; }

    public ProjectionVersion Version { get; }

    public DateTimeOffset? AsOf { get; }

    public PagingOptions PagingOptions { get; }

    public int BatchSize { get; }
}
