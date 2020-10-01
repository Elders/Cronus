# Ports

Port is the mechanism to establish communication between aggregates. Usually this involves one aggregate who triggered an event and one aggregate which needs to react.

If you feel the need to do more complex interactions, it is advised to use Saga. The reason for this is that ports do not provide a transparent view of the business flow because they do not have persistent state.

## Communication Guide Table

| Triggered by | Description |
| :--- | :--- |
| Event | Domain events represent business changes which have already happened |

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a port can send a command
{% endhint %}



