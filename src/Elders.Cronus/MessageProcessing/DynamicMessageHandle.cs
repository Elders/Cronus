using System.Threading.Tasks;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing;

/// <summary>
/// Work-flow which gets an object from the passed context and calls a method 'Handle' passing execution.Context.Message'
/// <see cref="HandlerContext"/> should have 'HandlerInstance' and 'Message' already set
/// </summary>
public sealed class DynamicMessageHandle : Workflow<HandlerContext>
{
    protected override Task RunAsync(Execution<HandlerContext> execution)
    {
        dynamic handler = execution.Context.HandlerInstance;
        return handler.HandleAsync((dynamic)execution.Context.Message);
    }
}
