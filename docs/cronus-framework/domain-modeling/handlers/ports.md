# Ports

[https://github.com/Elders/Cronus/issues/258](https://github.com/Elders/Cronus/issues/258)

Port is the mechanism to establish communication between aggregates. Usually, this involves one aggregate that triggered an event and one aggregate which needs to react.

If you feel the need to do more complex interactions, it is advised to use Saga. The reason for this is that ports do not provide a transparent view of the business flow because they do not have a persistent state.

## Communication Guide Table

| Triggered by | Description                                                         |
| ------------ | ------------------------------------------------------------------- |
| Event        | Domain events represent business changes that have already happened |

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a port can send a command
{% endhint %}

