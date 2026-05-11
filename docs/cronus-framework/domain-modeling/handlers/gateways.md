# Gateways

A gateway is a port that remembers things. It reacts to events and talks to an external system, just like a port, but in addition it maintains a small amount of persistent state that the external system — not the business — needs.

In Cronus, a gateway is any class that implements [`IGateway`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/IGateway.cs):

```csharp
public interface IGateway : IMessageHandler { }
```

The marker interface is minimal. A gateway earns its behaviour through [`IEventHandler<T>`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/IEventHandler.cs) implementations, just like a port or a saga.

## Gateway vs Port vs Projection

The XML doc on `IGateway` sums up the niche it fills:

> Compared to `IPort`, which can dispatch a command, an `IGateway` can do the same but it also has a persistent state. A scenario could be sending commands to external BC like push notifications, emails etc. There is no need to event source this state and it's perfectly fine if this state is wiped. Example: iOS push notifications badge. This state should be used only for infrastructure needs and never for business cases. Compared to projections, which track events and project their data and are not allowed to send any commands at all, an `IGateway` store and track a metadata required by external systems. Also, `IGateway` are restricted and not touched when events are replayed.

Three rules follow from that:

1. A gateway is the right choice when the **external system** needs state (an APNS badge count, a last-seen device id, a third-party handle). Business state belongs on aggregates.
2. A gateway is **not replayed**. Unlike projections, gateway state survives a projection rebuild. That makes it safe to store values that are meaningful only in combination with live external resources.
3. A gateway, unlike a projection, **may publish commands**. That is why it is shaped more like a port than a projection.

## Why a gateway encapsulates retries and serialisation

Calling an external service by hand from an application service is a trap: you either hand-roll retry, timeout and serialisation logic every time, or you forget it. A gateway is the canonical place to centralise those concerns for a given external system, so the application services above it stay clean.

In practice a gateway wraps the SDK of the external system (HTTP client, gRPC stub, legacy RPC proxy) and applies the retry policy you want for that system's failure modes.

## Example

```csharp
public class SampleGateway : IGateway,
    IEventHandler<SampleReserved>
{
    public Task HandleAsync(SampleReserved @event)
    {
        Console.WriteLine($"Sample with ID: '{@event.Id}' was reserved!");
        return Task.CompletedTask;
    }
}
```

A realistic gateway would inject the external-system client plus whatever persistent store holds the infrastructure metadata (for example a table of APNS device tokens and current badge counts).

## Configuration

The subscriber that dispatches events to gateways is toggled by `Cronus:GatewaysEnabled` (default: `true`). Disable it on hosts that should not talk to the external system, for instance when you split the host into a write-only and a read-only process.

{% content-ref url="../../configuration.md" %}
[configuration.md](../../configuration.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a gateway **can** publish new commands
* a gateway **can** call external services and SDKs
* a gateway **can** maintain persistent infrastructure state
* a gateway **must** tolerate the external service being slow or down
* a gateway **must** be idempotent — the same event may arrive more than once
{% endhint %}

{% hint style="warning" %}
**You should not...**

* a gateway **should not** hold business state — put that on an aggregate
* a gateway **should not** be touched during event replay — its state must not depend on event order
* a gateway **should not** be used when a plain port is enough; only reach for a gateway when the external system needs to persist metadata
{% endhint %}
