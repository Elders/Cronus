using System;

namespace Elders.Cronus.EventStore.Index;

public sealed class IndexRecord
{
    public IndexRecord(string id, ReadOnlyMemory<byte> aggregateRootId)
    {
        Id = id;
        AggregateRootId = aggregateRootId;
        Revision = 0;
        Position = 0;
        TimeStamp = 0;
    }

    public IndexRecord(string id, ReadOnlyMemory<byte> aggregateRootId, int revision, int position, long timestamp)
    {
        if (revision < 1) throw new ArgumentOutOfRangeException(nameof(revision), revision, "IndexRecord revision is out of range.");
        if (position < 0) throw new ArgumentOutOfRangeException(nameof(position), position, "IndexRecord position is out of range.");
        if (timestamp <= 0) throw new ArgumentOutOfRangeException(nameof(timestamp), timestamp, "IndexRecord timestamp is out of range.");

        Id = id;
        AggregateRootId = aggregateRootId;
        Revision = revision;
        Position = position;
        TimeStamp = timestamp;
    }

    public string Id { get; private set; }

    public ReadOnlyMemory<byte> AggregateRootId { get; private set; }

    public int Revision { get; private set; }

    public int Position { get; private set; }

    public long TimeStamp { get; private set; }

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}
