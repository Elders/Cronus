# Workflows

Workflows are the center of the message processing. It is very similar to the [ASP.NET middleware pipeline](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-3.1).

With a workflow you can:

* define what logic will be executed when a message arrives
* execute an action before or after the actual execution
* override or stop a workflow pipeline

## Default workflows

By default all messages are handled in an isolated fashion via [`ScopedMessageWorkflow`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/MessageProcessing/ScopedMessageWorkflow.cs) using scopes. Once the scope is created then the next workflow \([`MessageHandleWorkflow`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/MessageProcessing/MessageHandleWorkflow.cs)\) is invoked with the current message and scope. In addition, [`DiagnosticsWorkflow`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Workflow/DiagnosticsWorkflow.cs) wraps the entire pipeline bringing additional insights about the performance of the message handling pipeline.

#### ScopedMessageWorkflow

TODO: Explain scope purpose, IServiceProviderScope, Tenant, Context

#### MessageHandleWorkflow

TODO: Explain message handling workflow responsibilities

 



