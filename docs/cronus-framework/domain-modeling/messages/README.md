# Messages

A _message_ is the unit of communication that flows through a Cronus bounded context. Every message implements `IMessage` and carries a `Timestamp`. Cronus recognises four kinds of messages and each has its own role in the domain model.

| Message type      | Intent                                                                                      | Dispatched by           |
| ----------------- | ------------------------------------------------------------------------------------------- | ----------------------- |
| **Command**       | Request a business change. May be accepted or rejected by the aggregate.                    | API, saga, port         |
| **Event**         | Record a business change that already happened inside the bounded context.                  | Aggregate root / entity |
| **Public event**  | Announce a change to the outside world (published language). Carries the originating tenant. | Aggregate root          |
| **Signal**        | Trigger arbitrary side-effects (heartbeats, rebuilds, process pings).                       | Anything                |

{% hint style="info" %}
All messages get serialised across the transport. They **must** have a parameterless constructor and a `[DataContract]` attribute with a stable GUID `Name`. That GUID is the wire identity of the contract — do not change it once the message is in production.
{% endhint %}

{% content-ref url="../../messaging/serialization.md" %}
[serialization.md](../../messaging/serialization.md)
{% endcontent-ref %}

## Contracts at a glance

```csharp
public interface IMessage { DateTimeOffset Timestamp { get; } }

public interface ICommand     : IMessage { }
public interface IEvent       : IMessage { }
public interface IPublicEvent : IMessage { string Tenant { get; } }
public interface ISignal      : IMessage { }
public interface IScheduledMessage : IMessage { DateTime PublishAt { get; } }
```

Each kind is documented on its own page:

{% content-ref url="commands.md" %}
[commands.md](commands.md)
{% endcontent-ref %}

{% content-ref url="events.md" %}
[events.md](events.md)
{% endcontent-ref %}

{% content-ref url="public-events.md" %}
[public-events.md](public-events.md)
{% endcontent-ref %}

{% content-ref url="signals.md" %}
[signals.md](signals.md)
{% endcontent-ref %}

## Publishing

To send a message from outside an aggregate (for example from an API controller, a port, or a saga) inject the typed `IPublisher<T>` and `await` one of its `PublishAsync` overloads:

```csharp
Task<bool> PublishAsync(TMessage message, Dictionary<string, string> headers = null);
Task<bool> PublishAsync(TMessage message, DateTime publishAt, Dictionary<string, string> headers = null);
Task<bool> PublishAsync(TMessage message, TimeSpan publishAfter, Dictionary<string, string> headers = null);
```

{% hint style="success" %}
**You can / should / must**

* every message **must** be immutable
* every message **must** carry a stable `[DataContract(Name = "<guid>")]` identifier
* you **should** override `ToString()` — Cronus uses it when writing structured logs
* you **must** treat the return value of `PublishAsync` as the publish outcome; `false` means the transport rejected the message
{% endhint %}
