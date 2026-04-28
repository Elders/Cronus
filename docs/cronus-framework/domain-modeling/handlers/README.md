# Handlers

A handler is a class that reacts to an incoming message and produces some outcome. In Cronus, handlers come in six shapes, each with a different job.

Every handler implements the marker interface [`IMessageHandler`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/IMessageHandler.cs). Beyond that, the kind of handler determines which types of messages it listens to and what it is allowed to do in response.

## The six kinds

| Handler | Reacts to | Produces | Side effects allowed? |
| --- | --- | --- | --- |
| [Application Service](application-services.md) | `ICommand` | New events on a single aggregate | No — should not perform I/O outside the event store |
| [Projection](projections.md) | `IEvent` | A read model (snapshot or external store) | No — must not publish commands or events |
| [Saga](sagas.md) | `IEvent`, `IScheduledMessage` | New `ICommand` messages; scheduled timeouts | No business-facing side effects — coordinate aggregates |
| [Port](ports.md) | `IEvent` | New `ICommand` messages | Yes — the classic "send email", "call external API" place |
| [Trigger](triggers.md) | `IEvent`, `ISignal` | Anything — typically starts a job or a downstream workflow | Yes |
| [Gateway](gateways.md) | `IEvent` | New `ICommand` messages, with tracked infrastructure state | Yes — owns metadata required by an external system |

## Choosing a handler

The choice is about intent, not capability.

* Use an **Application Service** when a command must mutate one aggregate root.
* Use a **Projection** when you need a queryable read model built from the event stream.
* Use a **Saga** when the outcome of one event must lead to new commands that coordinate several aggregates.
* Use a **Port** when the outcome is a single follow-up command to another aggregate, or when you need a simple "when X happens, call the outside world" reaction.
* Use a **Trigger** when an event or signal should kick off a long-running job or workflow.
* Use a **Gateway** when you need a Port with a small amount of persistent infrastructure state — for example, last-known device token badges.

## Subscriber toggles

Each kind of handler is served by a dedicated subscriber that can be enabled or disabled per host process via [`CronusHostOptions`](../../configuration.md):

* `Cronus:ApplicationServicesEnabled`
* `Cronus:ProjectionsEnabled`
* `Cronus:SagasEnabled`
* `Cronus:PortsEnabled`
* `Cronus:TriggersEnabled`
* `Cronus:GatewaysEnabled`

All default to `true`. Turn one off when you want to split a monolith into specialised processes — for example, a projections host that never runs ports or sagas.

{% content-ref url="application-services.md" %}
[application-services.md](application-services.md)
{% endcontent-ref %}

{% content-ref url="projections.md" %}
[projections.md](projections.md)
{% endcontent-ref %}

{% content-ref url="sagas.md" %}
[sagas.md](sagas.md)
{% endcontent-ref %}

{% content-ref url="ports.md" %}
[ports.md](ports.md)
{% endcontent-ref %}

{% content-ref url="triggers.md" %}
[triggers.md](triggers.md)
{% endcontent-ref %}

{% content-ref url="gateways.md" %}
[gateways.md](gateways.md)
{% endcontent-ref %}
