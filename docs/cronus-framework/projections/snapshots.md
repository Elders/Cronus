# Projection snapshots

{% hint style="warning" %}
**Snapshots are not currently shipped with Cronus.** Earlier revisions of the framework had a snapshot subsystem keyed off a marker interface plus pluggable strategies, but those types were removed and no replacement has been merged. This page is kept so existing deep links keep resolving.
{% endhint %}

In the current codebase a projection's state is rebuilt by replaying every event of every type the projection handles — see [Versioning](versioning.md) and [Handlers / Projections](../domain-modeling/handlers/projections.md). There is no `ISnapshotStore`, no `IAmNotSnapshotable` marker, no `EventsCountSnapshotStrategy` / `TimeOffsetSnapshotStrategy` and no `Cronus:Projections:Cassandra:Snapshot…` configuration family in either [`Elders/Cronus`](https://github.com/Elders/Cronus) or [`Elders/Cronus.Projections.Cassandra`](https://github.com/Elders/Cronus.Projections.Cassandra) at the time of writing.

If a projection's event volume per id grows large enough that full replay becomes expensive, the practical lever today is the projection-versioning machinery — bumping a projection's hash forces a fresh rebuild, after which the new version serves all reads. See [Versioning](versioning.md) for that flow.

## Related

* [Versioning](versioning.md) — how a projection's shape change kicks off a replay.
* [Handlers / Projections](../domain-modeling/handlers/projections.md) — how to write the projection.
