using Elders.Cronus.EventStore;
using System;

namespace Elders.Cronus.Projections.Cassandra;

public readonly struct ProjectionQueryOptions(IBlobId id, ProjectionVersion version, PagingOptions pagingOptions, DateTimeOffset? asOf, int? batchSize)
{
    private const int DefaultBatchSize = 1000;

    public ProjectionQueryOptions(IBlobId id, ProjectionVersion version, DateTimeOffset asOf) : this(id, version, default, asOf, DefaultBatchSize) { }

    public ProjectionQueryOptions(IBlobId id, ProjectionVersion version, PagingOptions options) : this(id, version, options, null, options.Take) { }

    public IBlobId Id { get; } = id;

    public ProjectionVersion Version { get; } = version;

    public DateTimeOffset? AsOf { get; } = asOf;

    public PagingOptions PagingOptions { get; } = pagingOptions;

    public int BatchSize { get; } = batchSize ?? DefaultBatchSize;
}
