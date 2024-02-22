using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections;

public sealed class ProjectionStream
{
    private readonly ProjectionVersion version;
    private readonly IBlobId projectionId;
    private readonly IEnumerable<IEvent> events;

    ProjectionStream()
    {
        events = new List<IEvent>();
    }

    public ProjectionStream(ProjectionVersion version, IBlobId projectionId, IEnumerable<IEvent> events)
    {
        if (version is null) throw new ArgumentException(nameof(version));
        if (projectionId is null) throw new ArgumentException(nameof(projectionId));
        if (events is null) throw new ArgumentException(nameof(events));

        this.version = version;
        this.projectionId = projectionId;
        this.events = events;
    }

    public async Task<T> RestoreFromHistoryAsync<T>(T projection) where T : IProjectionDefinition
    {
        if (events.Any() == false)
            return default(T);

        IEnumerable<IEvent> eventsOrderedByTimestamp = events.OrderBy(@event => @event.Timestamp);

        projection.InitializeState(projectionId, null);
        foreach (IEvent @event in eventsOrderedByTimestamp)
        {
            await projection.ApplyAsync(@event).ConfigureAwait(false);    // Because of the order we need to await each event before replaying the next one.
        }

        return projection;
    }

    private readonly static ProjectionStream _emptyProjectionStream = new ProjectionStream();

    public static ProjectionStream Empty()
    {
        return _emptyProjectionStream;
    }
}
