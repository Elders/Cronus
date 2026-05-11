# Extensibility

Cronus is built on a reflection-based extensibility seam called **discoveries**. Every satellite package — `Cronus.Transport.RabbitMQ`, `Cronus.Persistence.Cassandra`, `Cronus.Projections.Cassandra`, and so on — ships one or more discovery classes. When `services.AddCronus(configuration)` is called at boot, Cronus scans the loaded assemblies, invokes every discovery it finds, and applies the resulting `ServiceDescriptor` entries to the host's DI container. Your own code can ship discoveries too; that is how you inject or replace services in a way that composes cleanly with the framework defaults.

On top of discoveries sit three more extensibility points, covered in the pages below:

* **`[CronusStartup]`** — a way to schedule `ICronusStartup` code to run at a particular phase of the boot sequence.
* **Fault handling** — the retry policies Cronus ships with, and how to replace them.
* **Observability** — the `DiagnosticListener`, `ActivitySource` and heartbeat subsystem Cronus wires up by default, and how to bolt on your own exporter.

## In this section

{% content-ref url="discoveries.md" %}
[discoveries.md](discoveries.md)
{% endcontent-ref %}

{% content-ref url="startup-attribute.md" %}
[startup-attribute.md](startup-attribute.md)
{% endcontent-ref %}

{% content-ref url="fault-handling.md" %}
[fault-handling.md](fault-handling.md)
{% endcontent-ref %}

{% content-ref url="observability.md" %}
[observability.md](observability.md)
{% endcontent-ref %}
