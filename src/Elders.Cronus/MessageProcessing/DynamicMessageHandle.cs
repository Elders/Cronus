using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    public class DynamicMessageHandle : Workflow<HandlerContext>
    {
        protected override void Run(Execution<HandlerContext> execution)
        {
            dynamic handler = execution.Context.HandlerInstance;
            handler.Handle((dynamic)execution.Context.Message);
        }
    }
}
