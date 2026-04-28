---
description: ES
---

# Event Sourcing

## What Event Sourcing is in Cronus

An event-sourced service does not persist the current state of its business objects. It persists the sequence of events that describe every change that ever happened to them, and derives the state on demand by replaying those events. Cronus treats the event store as the single source of truth for the write side of the service — everything else (aggregates in memory, projections, indices, analytical reports) is a derived artefact that can be thrown away and recomputed.

Contrast this with an EF- or ORM-based service. There you persist the current row of a `Concert` table, and a change is an `UPDATE`. Any insight about _how_ the concert got to its current shape — when performers were added, when the venue changed, which commands were accepted and rejected — is lost unless you manually write an audit trail. An event-sourced service gets the audit trail for free because the events _are_ the persistence format.

{% hint style="info" %}
State is a cached summary of a sequence of events. The events are the truth.
{% endhint %}

## The pattern

A Cronus aggregate inherits from `AggregateRoot<TState>` and changes its state by calling `Apply(IEvent)`:

```csharp
public class Concert : AggregateRoot<ConcertState>
{
    Concert() { }

    public Concert(string name, Venue venue, DateTimeOffset startTime, TimeSpan duration)
    {
        Apply(new ConcertAnnounced(...));
    }

    public void RegisterPerformer(Performer performer)
    {
        if (state.HasStarted) throw new InvalidOperationException("...");
        Apply(new PerformerRegistered(...));
    }
}
```

The state lives in a separate class with `When(TEvent)` handlers — the only place the state is allowed to change:

```csharp
public class ConcertState : AggregateRootState<Concert, ConcertId>
{
    public List<Performer> Performers { get; private set; } = new();

    public void When(ConcertAnnounced @event) { /* mutate state */ }
    public void When(PerformerRegistered @event) { /* append performer */ }
}
```

A command is turned into zero or more events by the aggregate; those events become an `AggregateCommit` in the event store. Rehydrating the aggregate later is symmetric: Cronus loads the stream of past events for that aggregate id, feeds them through `When`, and the state you started from is the state you left off at. For the full walkthrough see [Aggregate](../domain-modeling/aggregate.md).

## Why Cassandra is a natural store

An event-sourced workload has a very specific shape: every write is an append, and the vast majority of reads are _"give me all events for this aggregate id in revision order"_. Cassandra's storage model is a direct match — a wide row per aggregate id, ordered clustering by revision, append-only inserts, no updates. Reads are a single partition scan; writes do not compete for a shared row lock. Distribution and replication come for free because that is what Cassandra is for.

Cronus's canonical event-store backend is [`Cronus.Persistence.Cassandra`](https://github.com/Elders/Cronus.Persistence.Cassandra) and it has been in production use since 2013. See [Event Store](../event-store/README.md) for the details of how the contract is implemented on top of Cassandra.

## Rebuilding projections from events

The property you buy with event sourcing — that the event log is the source of truth and everything else is derived — is most visible at the projection layer. A projection is a read model that handlers update as events flow through the system. If you change the projection's shape, the old rows in its store no longer match the new code. With a relational model, that would mean writing a schema migration. With event sourcing, it means _bumping the projection version and replaying the events through the new handler_; see [Projections / Versioning](../projections/versioning.md).

The same property makes things like _"backfill a new subscriber with every event it cares about since the start of time"_ and _"recompute this analytical report after fixing a bug"_ routine. Both are a call to [`IEventStorePlayer.EnumerateEventStore`](../event-store/eventstore-player.md) with the right event-type filter.

## When event sourcing helps and when it does not

Event sourcing is not a free meal. The write side is more complex than a CRUD service, the read side requires an explicit projection pipeline, and the investment only pays off once you lean on the event log for rebuilding state, backfilling subscribers and auditing history. The shape of business that benefits most is also the shape DDD prefers — established, non-trivial, long-lived, with a rich set of distinct state transitions that people actually care about.

{% hint style="success" %}
* you are building software for an established and already operating business
* audit trails, history and "how did we get here" matter to the business
* new read models keep showing up and you need to rebuild them from historical data
{% endhint %}

{% hint style="danger" %}
* a single-service CRUD app over a small, stable set of entities
* a throwaway MVP where the events will never outlive the current code
{% endhint %}

See also [Domain-Driven Design](ddd.md) and [Command Query Responsibility Segregation](cqrs.md) — the three concepts reinforce each other.
