using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.MessageProcessing
{
    public class CreateScopedHandlerWorkflow : Workflow<HandleContext, IHandlerInstance>
    {
        protected override IHandlerInstance Run(Execution<HandleContext, IHandlerInstance> execution)
        {
            var scope = ScopedMessageWorkflow.GetScope(execution.Context);
            return new DefaultHandlerInstance(scope.ServiceProvider.GetRequiredService(execution.Context.HandlerType));
        }
    }
}
