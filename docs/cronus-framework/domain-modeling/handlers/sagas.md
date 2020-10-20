---
description: Sometimes called a Process Manager
---

# Sagas

When we have a workflow, which involves several aggregates it is recommended to have the whole process described in a single place such as а Saga/ProcessManager.

## Communication Guide Table

| Triggered by | Description |
| :--- | :--- |
| Event | Domain events represent business changes which have already happened |

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a saga **can** send new commands
{% endhint %}

