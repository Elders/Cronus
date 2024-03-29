# Workflows

[https://github.com/Elders/Cronus/issues/266](https://github.com/Elders/Cronus/issues/266)

Workflows are the center of message processing. It is very similar to the [ASP.NET middleware pipeline](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-3.1).

With a workflow you can:

* define what logic will be executed when a message arrives
* execute an action before or after the actual execution
* override or stop a workflow pipeline

## Default workflows

By default, all messages are handled in an isolated fashion via [`ScopedMessageWorkflow`](../../src/Elders.Cronus/MessageProcessing/ScopedMessageWorkflow.cs) using scopes. Once the scope is created then the next workflow ([`MessageHandleWorkflow`](../../src/Elders.Cronus/MessageProcessing/MessageHandleWorkflow.cs)) is invoked with the current message and scope. In addition, [`DiagnosticsWorkflow`](../../src/Elders.Cronus/Workflow/DiagnosticsWorkflow.cs) wraps the entire pipeline bringing insights into the performance of the message handling pipeline.

#### ScopedMessageWorkflow

The primary focus of the workflow is to prepare an isolated scope and context within which a message is being processed. Usually, you should not interact with this workflow directly.

The workflow creates an instance of [`IServiceScope`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicescope?view=dotnet-plat-ext-3.1) which allows using Dependency Injection in a familiar to a dotnet developer way. In addition, the workflow initializes an instance of [`CronusContext`](../../src/Elders.Cronus/MessageProcessing/CronusContext.cs) which holds information about the current [tenant ](domain-modeling/multitenancy.md)handling the message.

Additionally, Cronus uses structured logging and a new log scope is created every time a new message arrives so you could co-relate log messages.

{% hint style="info" %}
Read more about the [Dependency Injection](https://docs.microsoft.com/en-us/archive/msdn-magazine/2016/june/essential-net-dependency-injection-with-net-core) and [service lifetimes](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#service-lifetimes) if this is a new concept for you.
{% endhint %}

#### MessageHandleWorkflow

TODO: Explain message handling workflow responsibilities

&#x20;

