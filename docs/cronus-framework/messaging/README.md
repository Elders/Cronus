# Messaging

Messaging is the transport layer that ties the pieces of a Cronus service — and of a federation of Cronus services — together. Commands flow from the edge to the application services; events flow out of the aggregates to the projections, ports, sagas and triggers that react to them; signals coordinate across tenants and across hosts. All of that travels through a small, transport-agnostic contract.

## The shape of messaging in Cronus

Every message that crosses a boundary is wrapped in a [`CronusMessage`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/CronusMessage.cs). It carries the payload (a typed `IMessage` or the serialized `PayloadRaw` bytes plus a contract id) and a `Dictionary<string, string>` of headers — tenant, bounded context, causation id, traceparent, and the usual routing metadata. Every subscriber operates on `CronusMessage` and inspects those headers to decide how to handle the payload.

On the publishing side the entry point is [`IPublisher<TMessage>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/PublisherBase.cs), with a specialisation per message kind (`IPublisher<ICommand>`, `IPublisher<IEvent>`, `IPublisher<IPublicEvent>`, `IPublisher<IScheduledMessage>`, `IPublisher<ISystemSignal>`). Publishers are implemented by transport packages; the framework ships the shared pipeline types [`PublisherBase<TMessage>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/PublisherBase.cs) and [`Publisher<TMessage>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Publisher.cs) (which adds a linear retry policy on publish failure).

On the subscription side the entry point is [`ISubscriber`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/MessageProcessing/ISubscriber.cs):

```csharp
public interface ISubscriber
{
    string Id { get; }
    IEnumerable<Type> GetInvolvedMessageTypes();
    Type HandlerType { get; }
    Task ProcessAsync(CronusMessage message);
}
```

A subscriber declares the message types it is interested in and knows how to dispatch a `CronusMessage` into its handler. The abstract [`SubscriberBase`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/MessageProcessing/SubscriberBase.cs) wires the handler type's `DataContract` id as the subscriber's id — that is the stable value the transport uses to route messages and that appears in headers like `RecipientHandlers`.

Both sides meet in the consumer, [`IConsumer<T>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IConsumer.cs):

```csharp
public interface IConsumer<out T> where T : IMessageHandler
{
    Task StartAsync();
    Task StopAsync();
}
```

Each handler kind (application services, projections, sagas, ports, triggers, gateways) has its own consumer, which is started or stopped by the corresponding `Cronus:*Enabled` flag (see [Configuration](../configuration.md#cronus.applicationservicesenabled)).

## Workflows

A message is processed through a pipeline of [`Workflow`](../workflows.md) steps. The standard pipeline creates a scoped service provider and a per-message `CronusContext` ([`ScopedMessageWorkflow`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/MessageProcessing/ScopedMessageWorkflow.cs)), dispatches the message to the handler ([`MessageHandleWorkflow`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/MessageProcessing/MessageHandleWorkflow.cs)) and wraps everything in a diagnostic envelope ([`DiagnosticsWorkflow`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Workflow/DiagnosticsWorkflow.cs)). On the publishing side the pipeline sets tenant and bounded-context headers ([`CronusHeadersPublishHandler`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/PublisherBase.cs)), emits structured log lines ([`LoggingPublishHandler`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/PublisherBase.cs)) and propagates the OpenTelemetry traceparent ([`ActivityPublishHandler`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/PublisherBase.cs)).

## Transports

The transport abstraction sits behind `IPublisher<T>` and the consumer contracts, so the framework itself does not care which broker is under it. Cronus ships with two implementations:

* [`Cronus.Transport.RabbitMQ`](https://github.com/Elders/Cronus.Transport.RabbitMQ) — the canonical transport, marked `olympus`. One private RabbitMQ broker per trust boundary, with optional federation and an independent public broker for cross-boundary public events. Configuration lives under `Cronus:Transport:RabbitMQ:*` and `Cronus:Transport:PublicRabbitMQ:*` — see [Configuration](../configuration.md#cronus.transport.rabbitmq).
* [`Cronus.Transport.AzureServiceBus`](https://github.com/Elders/Cronus.Transport.AzureServiceBus) — a secondary transport used where Azure Service Bus is the operational default.

RabbitMQ is the recommended transport for new services. It is the transport we have exercised in production hardest, the federation story maps naturally to multi-tenant and multi-bounded-context deployments, and the `Cronus:Transport:RabbitMQ:Consumer:FanoutMode` switch lets you flip the semantics from _competing consumers_ to _every node sees every message_ without changing the application code.

## Serialization

Messages need to become bytes before they go over the wire, and bytes again when a subscriber picks them up. That responsibility lives behind `ISerializer`. See [Serialization](serialization.md) for the shipped implementations, the contract-id convention and the rules for keeping `DataContract` attributes stable.

## Related pages

{% content-ref url="serialization.md" %}
[serialization.md](serialization.md)
{% endcontent-ref %}

{% content-ref url="../workflows.md" %}
[workflows.md](../workflows.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **should** standardise on RabbitMQ for new services unless you have an operational reason to pick Azure Service Bus
* you **should** rely on `CronusMessage.Headers` for routing data; the payload is the business contract, not the routing contract
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** bypass the publisher pipeline; the pipeline is what stamps headers and trace ids
* you **should not** implement `ISubscriber` by hand when a handler kind already exists; write the handler and let the framework wire the subscriber
{% endhint %}
