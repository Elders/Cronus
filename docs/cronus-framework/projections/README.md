# Projections

A projection is a read model derived from the events in the [Event Store](../event-store/README.md). It subscribes to the event types it cares about, maintains its own state in its own store, and exposes query-shaped answers to the rest of the service through `IProjectionReader`. The tactical reference — how to _write_ a projection, the `ProjectionDefinition<TState, TId>` base class, the `Subscribe()` pattern — lives under [Domain Modeling / Handlers / Projections](../domain-modeling/handlers/projections.md).

This section covers the two topics that are orthogonal to _how_ you write the handler, and that Cronus owns on your behalf once your projection is deployed:

{% content-ref url="versioning.md" %}
[versioning.md](versioning.md)
{% endcontent-ref %}

How a projection evolves over its lifetime. The shape of the handler changes across releases — the signature of `HandleAsync`, the fields on the state, the set of events subscribed to. Cronus detects that the shape has changed, issues a new _version_ of the projection, replays the events through the updated handler into a new storage slot, and promotes the new slot to _live_ once the replay completes.

{% content-ref url="snapshots.md" %}
[snapshots.md](snapshots.md)
{% endcontent-ref %}

How a projection avoids re-reading its entire history every time it is queried. Snapshots are periodic captures of a projection instance's state written to an `ISnapshotStore`; when the projection is next loaded, Cronus restores from the most recent snapshot and only replays the events that landed after it.
