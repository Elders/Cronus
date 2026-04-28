# Observability

Cronus ships with three built-in pieces you can tap into:

1. A `DiagnosticListener` and an `ActivitySource` for distributed tracing.
2. Structured log scopes carrying the tenant, aggregate id, message id and handler name.
3. A heartbeat signal emitted on a configurable interval so monitors can tell the host is alive.

Everything below is wired up for you the moment you call `services.AddCronus(configuration)`.

## `DiagnosticListener` and `ActivitySource`

`AddCronus` calls [`AddOpenTelemetry`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/CronusServiceCollectionExtensions.cs) which registers the two low-level primitives that every distributed-tracing stack on .NET builds on:

```csharp
internal static IServiceCollection AddOpenTelemetry(this IServiceCollection services)
{
    // https://github.com/dotnet/aspnetcore/blob/f3f9a1cdbcd06b298035b523732b9f45b1408461/src/Hosting/Hosting/src/WebHostBuilder.cs#L334
    // By default aspnet core registers a DiagnosticListener and if we add our own you will loose the http insights
    // However, for worker services we need to register our own Listener.
    if (services.Any(x => x.ServiceType == typeof(DiagnosticListener)) == false)
    {
        services.AddSingleton<DiagnosticListener>(new DiagnosticListener("cronus"));

        services.AddSingleton<ActivitySource>(new ActivitySource("Elders.Cronus", "11.0.0"));
    }

    return services;
}
```

Two singletons are added — but only if no `DiagnosticListener` has been registered yet. On an ASP.NET Core host, ASP.NET Core registers its own listener first; Cronus piggy-backs on it. On a worker-service host, Cronus registers a new one named `cronus`. The `ActivitySource` is always named `Elders.Cronus` with version `11.0.0`.

### Activities emitted by Cronus

Two places start and stop `Activity` objects:

* **Publish path** — [`ActivityPublishHandler`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/PublisherBase.cs) in the publisher pipeline. It starts an activity named `Publish {messageTypeName}` for every outgoing message, propagates `telemetry_traceparent` through the message headers, and writes the completed activity to the diagnostic listener under the name `Elders.Cronus.Hosting.Workflow`.
* **Handle path** — [`DiagnosticsWorkflow<TContext>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Workflow/DiagnosticsWorkflow.cs) in every subscriber. It starts an activity named `{HandlerType}__{MessageType}` for every handler invocation, reads the incoming `telemetry_traceparent` so the span joins the upstream trace, and writes to the listener under the same activity name `Elders.Cronus.Hosting.Workflow`.

Every activity carries at least one tag:

* `cronus_messageId` — the ID of the `CronusMessage` the activity tracks.

### Log scopes

`DiagnosticsWorkflow` also enriches every log scope with:

* `cronus_tenant` — the tenant the message belongs to, via `Message.GetTenant()`.
* `cronus_arid` — the aggregate root id, when the message payload exposes one.

These constants are defined in [`Log`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Userfull/CronusLogger.cs):

```csharp
public static class Log
{
    public const string Tenant = "cronus_tenant";
    public const string AggregateId = "cronus_arid";
    public const string AggregateName = "cronus_arname";

    public const string MessageId = "cronus_messageId";
    public const string MessageData = "cronus_messageData";
    public const string MessageType = "cronus_messageType";

    public const string MessageHandler = "cronus_messageHandler";

    // ... plus job and projection keys
}
```

Because these are plain string constants used as log-scope keys and activity-tag keys, they flow through any structured logger and any OpenTelemetry exporter with no further configuration on Cronus's side. Your tenant and bounded-context dimensions are already there; all you have to do is scrape them.

## Subscribing your own listener

The built-in `DiagnosticListener` is named `cronus`. To observe everything Cronus emits, subscribe to it from your host:

```csharp
DiagnosticListener.AllListeners.Subscribe(new AllListenersObserver());

class AllListenersObserver : IObserver<DiagnosticListener>
{
    public void OnCompleted() { }
    public void OnError(Exception error) { }

    public void OnNext(DiagnosticListener listener)
    {
        if (listener.Name == "cronus")
        {
            listener.Subscribe(new CronusEventsObserver());
        }
    }
}

class CronusEventsObserver : IObserver<KeyValuePair<string, object>>
{
    public void OnCompleted() { }
    public void OnError(Exception error) { }

    public void OnNext(KeyValuePair<string, object> kv)
    {
        if (kv.Key == "Elders.Cronus.Hosting.Workflow" && kv.Value is Activity activity)
        {
            // record duration, tags, parent span id, etc.
        }
    }
}
```

For most production setups you will not write this yourself — you will let the OpenTelemetry SDK subscribe for you via `AddSource("Elders.Cronus")`.

## Turning on an OTLP exporter

A minimal OTLP wiring in a host that already calls `services.AddCronus(...)`:

```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("my-bounded-context"))
    .WithTracing(tracing => tracing
        .AddSource("Elders.Cronus")
        .AddOtlpExporter(otlp =>
        {
            otlp.Endpoint = new Uri("http://otel-collector:4317");
        }));
```

`AddSource("Elders.Cronus")` is the one line that matters — it tells the OpenTelemetry SDK to listen to the `ActivitySource` Cronus creates. Every handle and publish activity then goes out over OTLP, with the `cronus_messageId`, `cronus_tenant` and `cronus_arid` tags Cronus set.

## Heartbeat

Cronus also registers a background [heartbeat service](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/Heartbeat/CronusHeartbeatService.cs) that publishes an [`HeartbeatSignal`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/Heartbeat/HeartbeatSignal.cs) on a timer, carrying the current bounded-context name and the full tenant list. This is useful as a liveness probe from outside the host: downstream services that consume signals will either see the heartbeat on schedule, or infer that the host is dead.

```csharp
internal static IServiceCollection AddCronusHeartbeat(this IServiceCollection services)
{
    services.AddOptions<HeartbeatOptions, HeartbeaOptionsProvider>();
    services.AddSingleton<IHeartbeat, CronusHeartbeat>();
    services.AddHostedService<CronusHeartbeatService>();

    return services;
}
```

The interval is configured by `Cronus:Heartbeat:IntervalInSeconds` — an unsigned integer between `5` and `3600` seconds, defaulting to `5`. Validated by a `[Range]` attribute on [`HeartbeatOptions`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/Heartbeat/HeartbeatOptions.cs).

{% content-ref url="../configuration.md" %}
[configuration.md](../configuration.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **can** subscribe to the `cronus` `DiagnosticListener` for custom telemetry sinks
* you **should** call `AddSource("Elders.Cronus")` in your OpenTelemetry setup; it is the one-line integration
* you **should** keep the heartbeat interval short (`5`–`30` seconds) for production services and long (up to `3600`) for batch hosts
* you **must** enrich your log sinks to print `cronus_tenant` and `cronus_arid`; they are the two most useful dimensions in a multitenant event-sourced system
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **should not** register a second `DiagnosticListener` with the name `cronus`; there is already one
* you **should not** filter the heartbeat signal out of every exporter — it is often the only signal you have that a worker is alive
* you **should not** add extra tags by mutating `Activity.Current` inside a handler without checking whether it is null; the activity only exists when a listener is enabled
{% endhint %}
