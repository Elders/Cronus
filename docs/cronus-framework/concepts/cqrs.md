---
description: CQRS
---

# Command Query Responsibility Segregation

## What CQRS is in Cronus

CQRS is a structural decision: the write side of a service and the read side of a service are different programs. They have different types, different stores, different scaling characteristics, and — often — different hosts. They share the business domain and the event log; they do not share their runtime.

In Cronus the split is explicit:

* **Writes** happen against an [Aggregate](../domain-modeling/aggregate.md). A command arrives, an application service loads the aggregate from the [Event Store](../event-store/README.md), calls a method, and saves the resulting events. The write side is tuned for correctness and for append-only throughput.
* **Reads** happen against a [Projection](../domain-modeling/handlers/projections.md). Projections subscribe to events, update their own store, and are queried through `IProjectionReader`. The read side is tuned for query shape — each projection exists to serve one or a few UI or API reads cheaply.

Neither side knows about the other at the type level. An application service never references a projection type; a projection never issues a command. Between them sits the [Event Store](../event-store/README.md) and the [Messaging](../messaging/README.md) layer.

## Commands and queries differ at the wire

The division shows up in the message taxonomy. Every command in Cronus is an `ICommand` — a strongly-typed instance with a `[DataContract]` attribute, routed through `IPublisher<ICommand>`, delivered through the application-services consumer, handled by a method on an application service that loads an aggregate, mutates it and calls `SaveAsync`. See the [commands reference](../domain-modeling/messages/commands.md).

Queries, by contrast, do not travel on the bus at all. A query is a `GetAsync(IBlobId)` call against `IProjectionReader`, returning a `ReadResult<T>` — an in-process function call. The UI layer or the API controller injects the reader and asks; no routing, no broker, no retry policy, no "eventually delivered". The read side is synchronous from the caller's point of view and the latency is bounded by how fast the projection store can answer.

That distinction is load-bearing. Commands have to be durable, at-least-once, and ordered per aggregate — they are business intent. Queries have to be cheap and right-now — they are user experience. Treating them as the same kind of thing would compromise both.

## Independent scaling and evolution

Because the two sides are runtime-separate, they scale independently. A read-heavy service might run three projection hosts and one application-services host; a write-heavy migration might run a single projection host and ten application-services hosts. The [`Cronus:*Enabled`](../configuration.md#cronus.applicationservicesenabled) flags are exactly the knobs: turn `ApplicationServicesEnabled` off on a host and it becomes a pure projection reader; turn `ProjectionsEnabled` off and it becomes a pure command processor.

Evolution is similarly decoupled. A new projection can be added without touching the aggregate — you add the projection type, deploy, let it catch up from the event store. A new command can be added without touching existing projections — the new event it produces only affects projections that subscribe to it. This is the operational payoff of CQRS, and it is the reason Cronus makes the split structural rather than stylistic.

## Independence from event sourcing

CQRS does not require event sourcing and event sourcing does not require CQRS, but in a Cronus service they always appear together. The event log is what lets the read side be rebuildable — a new projection version replays events through its updated handler; see [Projections / Versioning](../projections/versioning.md). Without the event log, a CQRS service would need a separate write-to-read synchronisation mechanism, and without the CQRS split, the event log would be trying to serve both the latest-state query and the append-only write path at once. Cronus picks the combination that makes the most of both.

## What each side owns

A useful way to think about it:

* The write side owns the _rules_. Can this performer be added to this concert? The aggregate knows. Can this order be cancelled after shipping? The aggregate knows. Rules are enforced by `AggregateRoot` methods that either produce an event or throw.
* The read side owns the _shape_. What does the list of upcoming concerts look like to a mobile client? A projection. What does the ops dashboard show? A projection. The read side is free to denormalise, aggregate across multiple aggregates, and expose exactly the shape the consumer wants.

Rules are written once and do not change shape with the UI. Shapes change often and do not change the rules. That is why keeping them in separate pipelines pays.

## Related

See also [Event Sourcing](es.md) and [Domain-Driven Design](ddd.md) — the three concepts reinforce each other; they are the load-bearing assumptions behind every Cronus service.
