# Projection markers

Most projections should participate in versioning and replay — that is the point of [`ProjectionVersionManager`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/ProjectionVersionManager.cs). A few should not. The framework expresses that with two marker interfaces declared in [`CronusAssembly.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/CronusAssembly.cs): `INonVersionableProjection` and `INonRebuildableProjection`. Both are opt-out signals — you implement them on your projection class, and the framework changes how it treats the projection at boot and during replay.

This page covers when to reach for either marker, what changes when you do, and how the markers interact with the rest of the versioning subsystem.

## The problem the markers solve

The default versioning policy ([`MarkupInterfaceProjectionVersioningPolicy`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/MarkupInterfaceProjectionVersioningPolicy.cs)) treats every projection as versionable. Whenever the projection's hash changes, the version manager requests a new version, the builder replays every event the projection handles, and once the new version is live, reads switch over. That is the right thing for almost every projection.

It is the wrong thing for two narrow cases:

1. **System projections that the versioning subsystem itself depends on.** [`ProjectionVersionsHandler`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/Handlers/ProjectionVersionsHandler.cs) is the projection that *records* what versions exist. If it tried to participate in versioning, the system would have to be running before it could be running.
2. **Projections that should not be wiped and rebuilt at all** — typically because their state cannot be reconstructed from events alone, or because rebuilding them would be ruinously expensive and the on-disk shape is stable enough that no rebuild is ever needed.

`INonVersionableProjection` covers the first; `INonRebuildableProjection` covers the second.

## INonVersionableProjection

The full interface:

```csharp
public interface INonVersionableProjection
{
    string GetHash() => "ver";

    int GetRevision() => 1;
}
```

When a projection implements this marker, two things change:

* [`ProjectionHasher.CalculateHash`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/ProjectionHasher.cs) does not derive a hash from the set of `IEventHandler<T>` interfaces. It instantiates the projection with the parameterless constructor and returns whatever `GetHash()` says — `"ver"` by default. Adding or removing event handlers does not change this value.
* `MarkupInterfaceProjectionVersioningPolicy.IsVersionable(projectionName)` returns `false`. The version manager — which reads the policy on every `Replay`, `NotifyHash` and `Rebuild` call — treats the projection as non-versionable, which means it does not request a new version when the handler set changes.

The combined effect is that the projection has exactly one version for its lifetime. Reads always come from that version's storage slot; replays do not happen automatically when you change the code.

The framework uses this on `ProjectionVersionsHandler` because that projection has to be available before the version manager can do anything; there is no chicken-and-egg way to version it. Your code uses it when you have a projection whose state must survive deploys verbatim — for example, a projection that records the latest known offset of an external feed, where re-deriving the value from the event log is not the right behaviour even if you change the handler shape.

A worked example:

```csharp
[DataContract(Name = "8a4f06d3-3f9b-46ab-8f5c-6f6b6e64a3f5")]
public class FeedOffsetProjection : ProjectionDefinition<FeedOffsetState, FeedId>,
    INonVersionableProjection,
    IEventHandler<FeedAdvanced>
{
    public FeedOffsetProjection()
    {
        Subscribe<FeedAdvanced>(x => new FeedId(x.Id.Tenant, x.Id.Id));
    }

    public Task HandleAsync(FeedAdvanced @event)
    {
        // ...
        return Task.CompletedTask;
    }
}
```

If you later add `IEventHandler<FeedReset>`, the hash stays `"ver"`, no replay is requested, and the new handler simply starts running for new events from the moment it is deployed. State accumulated under the previous handler set is preserved.

## INonRebuildableProjection

The interface is empty — it is a pure marker:

```csharp
public interface INonRebuildableProjection { }
```

The semantics are "this projection's state must not be wiped and reconstructed from the event log". A projection that cannot survive a rebuild is one where, for example, the state was bootstrapped from a one-off import that the event log does not contain, or where part of the state is computed from an external source at write time and the source is no longer available.

The framework declares the marker but does not currently apply it inside the versioning policy classes that ship in [`Elders.Cronus`](https://github.com/Elders/Cronus). The Cronus tests ([`TestData.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus.Tests/Projections/TestData.cs)) cover the type so satellite packages — particularly the projection store implementations such as [`Elders.Cronus.Projections.Cassandra`](https://github.com/Elders/Cronus.Projections.Cassandra) — can detect it and refuse to drop or recreate the projection's tables during a rebuild.

The recommended treatment, until the marker has more first-class support in core, is to use it as a **declaration of intent** in your domain code so that operators reading the projection know not to issue a rebuild against it. Wire a real refusal — failing the call, or branching at the projection-store level — in whatever component manages your rebuild operations.

```csharp
[DataContract(Name = "1d6f2c79-1e8b-4a07-9db6-2e0c8b0f2d3f")]
public class ImportedCustomerLedgerProjection : ProjectionDefinition<ImportedCustomerLedgerState, CustomerId>,
    INonRebuildableProjection,
    IEventHandler<LedgerEntryRecorded>
{
    // ...
}
```

## Interaction with the version manager

The version manager runs the same flow regardless of markers — it requests versions, it tracks timeboxes, it transitions a `Building` version to `Live`. The markers change two of its inputs:

* [`ProjectionHasher.CalculateHash`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/ProjectionHasher.cs) returns the marker-overridden hash for `INonVersionableProjection`, so the manager never sees a hash change for those projections in normal operation.
* [`MarkupInterfaceProjectionVersioningPolicy.IsVersionable`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/MarkupInterfaceProjectionVersioningPolicy.cs) returns `false` for `INonVersionableProjection`, which is what the manager uses to decide whether `Replay` is allowed. The first-time-ever case is special-cased: even a non-versionable projection gets a single creation pass when no live version exists yet.

The disaster-recovery branch in [`ProjectionVersionManager.Rebuild`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/ProjectionVersionManager.cs) bypasses the policy entirely — it is reserved for the system projection that owns versioning state.

## Cross-link

The full versioning lifecycle, including how the policy is consulted and how the timebox is set, lives next to this page:

{% content-ref url="versioning.md" %}
[versioning.md](versioning.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can / should / must**

* a projection **must** be marked `INonVersionableProjection` if its state cannot survive a routine handler-set change — the default behaviour is to wipe and replay
* a projection **can** override `GetHash()` on `INonVersionableProjection` to bump the version explicitly when you really do want a one-off rebuild
* a projection **should** be marked `INonRebuildableProjection` if the projection-store side has no way to reconstruct it — the marker is what operators look for
* the marker decision **should** live in the same file as the projection class so the constraint is visible at every reading
{% endhint %}

{% hint style="warning" %}
**You should not**

* a projection **should not** be marked `INonVersionableProjection` to "save time" — versioning is the framework's mechanism for keeping the read model honest
* a projection **should not** depend on `INonRebuildableProjection` having a framework-enforced effect today; treat the marker as documentation backed by your projection-store integration
* a projection **should not** mix the markers without a reason; pick the one that matches the constraint you want to express
{% endhint %}
