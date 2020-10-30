# Gateways

[https://github.com/Elders/Cronus/issues/260](https://github.com/Elders/Cronus/issues/260)

Compared to Port, which can dispatch a command, a Gateway can do the same but it also has a persistent state. A scenario could be sending commands to external BC, such as push notifications, emails etc. There is no need to event source this state and its perfectly fine if this state is wiped. Example: iOS push notifications badge. This state should be used only for infrastructure needs and never for business cases. Compared to Projection, which tracks events, projects their data, and are not allowed to send any commands at all, a Gateway can store and track metadata required by external systems. Furthermore, Gateways are restricted and not touched when events are replayed.

## Communication Guide Table

| Triggered by | Description |
| :--- | :--- |
| Event | Domain events represent business changes which have already happened |

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a gateway **can** send new commands
{% endhint %}

