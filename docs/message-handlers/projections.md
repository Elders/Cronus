# Projections

Projection tracks events and project their data for specific purposes.

## Communication Guide Table

| Triggered by | Description |
| :--- | :--- |
| Event | Domain events represent business changes which have already happened |

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a projection **must** be idempotent
* a projection **must not** issue new commands or events
{% endhint %}

{% hint style="warning" %}
**You should not...**

* a projection **should not** query other projections. All the data of a projection must be collected from the Events' data
* a projection **should not** establish calls to external systems
{% endhint %}





