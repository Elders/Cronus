using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.MessageProcessing;

/// <summary>
/// Workflow that resolves a scoped <see cref="IHandlerInstance"/> from the per-message service scope.
/// </summary>
public class CreateScopedHandlerWorkflow : Workflow<HandleContext, IHandlerInstance>
{
    /// <inheritdoc />
    protected override Task<IHandlerInstance> RunAsync(Execution<HandleContext, IHandlerInstance> execution, CancellationToken cancellationToken = default)
    {
        IServiceScope scope = ScopedMessageWorkflow.GetScope(execution.Context);
        IHandlerInstance handler = new DefaultHandlerInstance(scope.ServiceProvider.GetRequiredService(execution.Context.HandlerType));
        return Task.FromResult(handler);
    }
}
