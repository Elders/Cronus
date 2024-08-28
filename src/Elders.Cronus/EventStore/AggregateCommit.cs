using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Elders.Cronus.EventStore;

public sealed class AggregateCommitRaw : IMessage
{
    public AggregateCommitRaw(IEnumerable<AggregateEventRaw> @events, DateTimeOffset timestamp)
    {
        Events = new List<AggregateEventRaw>(@events);
        Timestamp = timestamp;
    }

    public List<AggregateEventRaw> Events { get; private set; }

    /// <summary>
    /// This is the timestamp of commit creation.
    /// This value is different from the timestamp of concrete events.
    /// Usage: only for infrastructore purpoces
    /// </summary>
    public DateTimeOffset Timestamp { get; private set; }
}

[DataContract(Name = "f69daa12-171c-43a1-b049-be8a93ff137f")]
public class AggregateCommit : IMessage
{
    AggregateCommit()
    {
        Events = new List<IEvent>();
        PublicEvents = new List<IPublicEvent>();
    }

    public AggregateCommit(AggregateCommit sourceToCopy) : this(sourceToCopy.AggregateRootId, sourceToCopy.Revision, sourceToCopy.Events, sourceToCopy.PublicEvents, sourceToCopy.Timestamp)
    {
    }

    public AggregateCommit(ReadOnlyMemory<byte> aggregateRootId, int revision, List<IEvent> events, List<IPublicEvent> publicEvents, long timestamp)
    {
        if (revision < 1) throw new ArgumentOutOfRangeException(nameof(revision), revision, "Aggregate commit revision is out of range.");
        if (timestamp <= 0) throw new ArgumentOutOfRangeException(nameof(timestamp), timestamp, "Aggregate commit timestamp is out of range.");

        AggregateRootId = aggregateRootId;
        Revision = revision;
        Events = events;
        PublicEvents = publicEvents;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public ReadOnlyMemory<byte> AggregateRootId { get; private set; }

    [Obsolete("Do not use this.", true)]
    [DataMember(Order = 2)]
    string BoundedContext { get; set; }

    [DataMember(Order = 3)]
    public int Revision { get; private set; }

    [DataMember(Order = 4)]
    public List<IEvent> Events { get; set; }

    [DataMember(Order = 6)]
    public List<IPublicEvent> PublicEvents { get; set; }

    /// <summary>
    /// This is the timestamp of commit creation.
    /// This value is different from the timestamp of concrete events.
    /// Usage: only for infrastructore purpoces
    /// </summary>
    [DataMember(Order = 5)]
    public long Timestamp { get; private set; }

    DateTimeOffset IMessage.Timestamp => throw new NotImplementedException();

    public override string ToString()
    {
        StringBuilder commitInfoBuilder = new StringBuilder();
        commitInfoBuilder.AppendLine("AggregateCommit details:");

        commitInfoBuilder.Append("RootId:");
        commitInfoBuilder.AppendLine(Encoding.UTF8.GetString(AggregateRootId.Span));

        commitInfoBuilder.Append("Revision:");
        commitInfoBuilder.AppendLine(Revision.ToString());

        commitInfoBuilder.Append("Events:");
        commitInfoBuilder.AppendLine(string.Join(Environment.NewLine, Events.Select(e => "\t" + e.ToString())));

        commitInfoBuilder.Append("PublicEvents:");
        commitInfoBuilder.AppendLine(string.Join(Environment.NewLine, PublicEvents.Select(e => "\t" + e.ToString())));

        return commitInfoBuilder.ToString();
    }
}
