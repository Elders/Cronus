using System;

namespace Elders.Cronus.Projections.Cassandra
{
    public readonly struct ProjectionQueryOptions(IBlobId id, ProjectionVersion version, DateTimeOffset? asOf, int? batchSize)
    {
        private const int DefaultBatchSize = 1000;

        public ProjectionQueryOptions(IBlobId id, ProjectionVersion version, DateTimeOffset asOf) : this(id, version, asOf, DefaultBatchSize) { }

        public ProjectionQueryOptions(IBlobId id, ProjectionVersion version) : this(id, version, null, DefaultBatchSize) { }

        public IBlobId Id { get; } = id;

        public ProjectionVersion Version { get; } = version;

        public DateTimeOffset? AsOf { get; } = asOf;

        public int BatchSize { get; } = batchSize ?? DefaultBatchSize;
    }
}
