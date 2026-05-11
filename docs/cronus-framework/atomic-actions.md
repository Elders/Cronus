# Atomic Actions

An atomic action is the cross-process lock Cronus takes around the "load aggregate, append events" critical section. It is the safety net that turns optimistic event-store concurrency into something safe to run on multiple hosts at once: at most one node, anywhere in the cluster, may append the next revision of a given aggregate at any given moment.

You will see two interfaces under [`Elders.Cronus.AtomicAction`](https://github.com/Elders/Cronus/tree/master/src/Elders.Cronus/AtomicAction) — [`IAggregateRootAtomicAction`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/AtomicAction/IAggregateRootAtomicAction.cs) and [`ILock`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/AtomicAction/ILock.cs). The first is what `AggregateRepository.SaveAsync` uses; the second is the primitive lock that the atomic action is built on. Most projects only ever wire up an implementation; very few write a new one.

## When you need it

Read paths do not need an atomic action — they replay the stream and produce a state. Write paths do, because two hosts that load the same aggregate at the same revision and both try to append revision `N+1` would otherwise race. The Cronus event store catches a duplicate revision and rejects the second writer with [`AggregateStateFirstLevelConcurrencyException`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/AtomicAction/AggregateStateFirstLevelConcurrencyException.cs) — but the lost work has already happened. The atomic action stops the second writer earlier, before any side effect runs.

You need a real implementation as soon as more than one process can write to the same aggregate. A single-process worker can run on the missing implementation; an Aspire-era Cronus solution where API hosts and worker hosts both call `SaveAsync` cannot.

## The interfaces

`IAggregateRootAtomicAction` is the contract `AggregateRepository` calls. The full surface is one method:

```csharp
public interface IAggregateRootAtomicAction : IDisposable
{
    Task<Result<bool>> ExecuteAsync(AggregateRootId arId, int aggregateRootRevision, Func<Task> action);
}
```

`ExecuteAsync` is given the aggregate id, the revision the caller intends to append, and the work that should run while the lock is held. The implementation acquires a lock keyed on `arId.Value`, runs `action`, releases the lock and returns a [`Result<bool>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Userfull/Result.cs) — `IsSuccessful = true` if both the lock and the action succeeded.

`ILock` is the lower-level primitive the atomic action sits on:

```csharp
public interface ILock
{
    Task<bool> IsLockedAsync(string resource);
    Task<bool> LockAsync(string resource, TimeSpan ttl);
    Task UnlockAsync(string resource);
}
```

A lock is named (`resource`) and has a time-to-live. `LockAsync` returns `true` when the lock was acquired. `UnlockAsync` releases it explicitly; the TTL releases it implicitly if the holder dies. The same `ILock` instance is reused across many atomic actions.

## Configuration keys

The Redis-backed implementation binds two option classes against the `cronus:atomicaction:redis` section. The full reference, including defaults and validation, lives next to the rest of the framework configuration:

{% content-ref url="configuration.md" %}
[configuration.md](configuration.md)
{% endcontent-ref %}

Specifically: [`Cronus:AtomicAction:Redis:ConnectionString`](configuration.md#cronus-atomicaction-redis-connectionstring), [`Cronus:AtomicAction:Redis:LockTtl`](configuration.md#cronus-atomicaction-redis-lockttl), [`Cronus:AtomicAction:Redis:LongTtl`](configuration.md#cronus-atomicaction-redis-longttl), [`Cronus:AtomicAction:Redis:LockRetryCount`](configuration.md#cronus-atomicaction-redis-lockretrycount), [`Cronus:AtomicAction:Redis:LockRetryDelay`](configuration.md#cronus-atomicaction-redis-lockretrydelay) and [`Cronus:AtomicAction:Redis:ClockDriveFactor`](configuration.md#cronus-atomicaction-redis-clockdrivefactor).

`LockTtl` is the short TTL applied while the action runs; `LongTtl` is the longer TTL applied after the action succeeds, to prevent a late-arriving node from re-running the same revision. The retry count and delay are passed through to RedLock; the clock-drive factor compensates for clock drift between Redis nodes.

## Choosing an implementation

Three implementations exist. The first is the only one you should default to.

* [**Redis**](https://github.com/Elders/Cronus.AtomicAction.Redis) — the production choice. [`RedisAggregateRootAtomicAction`](https://github.com/Elders/Cronus.AtomicAction.Redis/blob/master/src/Elders.Cronus.AtomicAction.Redis/RedisAggregateRootAtomicAction.cs) wraps a [`RedisAggregateRootLock`](https://github.com/Elders/Cronus.AtomicAction.Redis/blob/master/src/Elders.Cronus.AtomicAction.Redis/AggregateRootLock/RedisAggregateRootLock.cs) (an `ILock` over `Elders.RedLock` 9.0.2 — the [Redlock algorithm](https://redis.io/topics/distlock)) plus an [`IRevisionStore`](https://github.com/Elders/Cronus.AtomicAction.Redis/blob/master/src/Elders.Cronus.AtomicAction.Redis/RevisionStore/IRevisionStore.cs) that remembers the last revision committed for each aggregate. Together they reject a stale `aggregateRootRevision` even if the lock acquisition itself was successful. The discovery — [`RedisAggregateRootAtomicActionDiscovery`](https://github.com/Elders/Cronus.AtomicAction.Redis/blob/master/src/Elders.Cronus.AtomicAction.Redis/RedisAggregateRootAtomicActionDiscovery.cs) — registers both `IAggregateRootAtomicAction` and `ILock` with `CanOverrideDefaults = true`, so adding the package is enough to replace the in-memory default.
* [**Consul**](https://github.com/Elders/Cronus.AtomicAction.Consul) — historical. The Consul implementation predates Cronus 11 and references the removed `IAggregateRootId` interface; it does not currently compile against Cronus 11.x and is not currently shipped as a working option. If you need a non-Redis implementation, use it as a sketch rather than as something you can drop in.
* [`MissingAggregateRootAtomicAction`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/AtomicAction/MissingAggregateRootAtomicAction.cs) — the in-process default. It implements both `IAggregateRootAtomicAction` and `ILock` by throwing `NotImplementedException` with a clear message: "The AggregateRootAtomicAction is not configured. Please install a nuget package which provides aggregate sync capabilities such as IAggregateRootAtomicAction. ex.: Cronus.AtomicAction.Redis." It is registered by Cronus core so the container resolves; the first save call is what fails. Treat it as "atomic actions are not wired up yet", not as "atomic actions are off".

## Failure modes

* **Lock not acquired.** `LockAsync` returned `false`. `RedisAggregateRootAtomicAction.ExecuteAsync` returns `Result<bool>(false).WithError("Failed to lock and execute atomic action.")`. `AggregateRepository.SaveInternalAsync` raises `AggregateStateFirstLevelConcurrencyException` from the result errors. The caller — typically an application service — sees the exception and is expected to either retry or abort the command.
* **Revision mismatch.** The lock was acquired but the revision the caller wants to append does not follow the last one the revision store remembers. The Redis implementation rolls back to the previous revision under `LongTtl` and returns failure. Same exception path as above.
* **Action threw.** The action ran but threw. The atomic action captures it (`Result.Error(ex)`), releases the lock, and the exception surfaces back through `SaveInternalAsync`.
* **Lock holder died.** Whoever held the lock crashed before calling `UnlockAsync`. The TTL releases it; the next caller acquires cleanly. This is why `LockTtl` defaults to one second — long enough for the work, short enough that nobody waits long for a dead holder.

## Best Practices

{% hint style="success" %}
**You can / should / must**

* an atomic action **must** be configured on every process that calls `IAggregateRepository.SaveAsync`
* an atomic action **must** key its lock on `AggregateRootId.Value` so different aggregates do not contend
* a Redis atomic action **should** sit on a Redis instance with the same availability profile as the event store — if Redis is gone, every write fails
* an `ILock` **can** be reused as a generic distributed-lock primitive within the same bounded context; the `ILock` registration is independent of `IAggregateRootAtomicAction`
{% endhint %}

{% hint style="warning" %}
**You should not**

* an atomic action **should not** be configured with a `LockTtl` longer than the slowest expected `SaveAsync` — a long TTL turns a crash into a long stall
* an atomic action **should not** be skipped on "read-only" hosts that occasionally write — there is no such thing as "occasional" when it comes to event-store concurrency
* a process **should not** rely on `MissingAggregateRootAtomicAction` for anything other than the very first integration test; ship a real implementation before more than one writer exists
{% endhint %}
