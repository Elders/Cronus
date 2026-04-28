# Workflows

Workflows are Cronus's message-processing pipeline. When a command, event, signal or scheduled message arrives from the transport, a workflow is responsible for resolving the right handler, invoking it inside the right scope, and surfacing any failures. The design mirrors the [ASP.NET Core middleware pipeline](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/): each workflow wraps an inner workflow and can add cross-cutting behaviour — logging, activity tracing, retries — before or after the inner `RunAsync` call.

## The building block

Every workflow inherits `Workflow<TContext>`:

```csharp
public abstract class Workflow<TContext> : WorkflowBase<TContext> where TContext : class
{
    protected abstract Task RunAsync(Execution<TContext> execution);
}
```

The context is the envelope that carries state down the pipeline. For message handling the context is `HandleContext`, which holds the `CronusMessage`, the handler type, and (once the scope has been created) the scoped `IServiceProvider`.

```csharp
public class HandleContext : IWorkflowContextWithServiceProvider
{
    public CronusMessage Message { get; }
    public Type HandlerType { get; }
    public IServiceProvider ServiceProvider { get; set; }
}
```

## The default message pipeline

When Cronus starts a subscriber (application services, projections, sagas, ports, triggers, gateways, indices) it composes a pipeline like this, from outer to inner:

1. **`ExceptionEaterWorkflow<HandleContext>`** — last line of defence; swallows and logs exceptions that escape the rest of the pipeline so the consumer does not crash.
2. **`DiagnosticsWorkflow<HandleContext>`** — starts a `System.Diagnostics.Activity`, emits a structured log scope containing the tenant and aggregate root id, and writes a `handled in Xms` info log on success.
3. **`InMemoryRetryWorkflow<HandleContext>`** — retries transient failures in-process before giving up.
4. **`ScopedMessageWorkflow`** — creates a fresh `IServiceScope` for the message, initialises a `CronusContext` with the tenant resolved from the headers, and attaches a logger scope. The inner workflow receives the scoped `ServiceProvider` via `HandleContext.ServiceProvider`.
5. **`MessageHandleWorkflow`** — resolves the handler via `CreateHandler`, invokes `BeginHandle` → `ActualHandle` → `EndHandle`, and routes exceptions into the `Error` sub-workflow. The default `ActualHandle` calls `DynamicMessageHandle` which dispatches to `HandleAsync(message)` on the resolved handler.

You can see the composition in [`ApplicationServiceSubscriberWorkflow`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/MessageProcessing/ApplicationServiceSubscriberWorkflow.cs); each subscriber kind has its own factory that assembles a pipeline tailored to its needs.

## Why it is generic

Each workflow is parameterised by its context type so the compiler can enforce that a `DiagnosticsWorkflow<HandleContext>` only wraps another `Workflow<HandleContext>`. The generic parameter is the context, not the message — there is one `DiagnosticsWorkflow<T>` type that works for any context that derives from `HandleContext`.

```csharp
public sealed class DiagnosticsWorkflow<TContext> : Workflow<TContext>
    where TContext : HandleContext
{
    public DiagnosticsWorkflow(
        Workflow<TContext> workflow,
        DiagnosticListener diagnosticListener,
        ActivitySource activitySource) { ... }
}
```

## Customising the pipeline

`MessageHandleWorkflow` exposes five extension points — each is itself a `Workflow` that defaults to a no-op lambda:

| Extension point | When it runs                                                    | Typical use                           |
| --------------- | --------------------------------------------------------------- | ------------------------------------- |
| `BeginHandle`   | Before the actual handler invocation                            | Instrumentation, authorisation checks |
| `ActualHandle`  | The handler call itself (default: `DynamicMessageHandle`)       | Replace or wrap dispatch logic        |
| `EndHandle`     | After a successful handler call                                 | Post-commit hooks, metrics            |
| `Error`         | When `BeginHandle` / `ActualHandle` / `EndHandle` throws        | Error enrichment, dead-letter routing |
| `Finalize`      | Always, at the end (after success or after `Error`)              | Cleanup                               |

Use `OnHandle(...)` to inject your own `Workflow<HandlerContext>` around the actual handler call:

```csharp
messageHandleWorkflow.OnHandle(inner =>
    WorkflowExtensions.Lamda<HandlerContext>()
        .Use(async ctx =>
        {
            // pre-handle
            await inner.RunAsync(ctx.Context).ConfigureAwait(false);
            // post-handle
        }));
```

{% hint style="info" %}
You rarely need to touch workflows directly. Reach for the extensibility points (ports, sagas, triggers, gateways) first — workflows are the primitive behind those abstractions.
{% endhint %}

## Diagnostics and tracing

`DiagnosticsWorkflow` writes Activities to the `Elders.Cronus` `ActivitySource` and a `DiagnosticListener` named `"cronus"`. If you configure OpenTelemetry (or any other APM) to listen for that source, you get end-to-end traces of every message handled in your host.

Log scopes include:

* `cronus_messageHandler` — the handler type name
* `cronus_messageType` — the message payload type name
* `cronus_tenant` — the tenant extracted from the message headers
* `cronus_arid` — the aggregate root id if the message carries one

{% hint style="success" %}
**You can / should / must**

* you **can** wrap the default `MessageHandleWorkflow` with your own `BeginHandle` / `EndHandle` to add cross-cutting behaviour
* you **should** keep workflow code synchronous-friendly — `DiagnosticListener` writes happen on the calling thread
* you **must** return `Task` promptly; any blocking operation in a workflow blocks the consumer
{% endhint %}
