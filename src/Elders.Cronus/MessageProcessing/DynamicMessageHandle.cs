using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessing
{
    public class DynamicMessageHandle : Middleware<HandlerContext>
    {
        protected override void Run(Execution<HandlerContext> execution)
        {
            dynamic handler = execution.Context.HandlerInstance;
            handler.Handle((dynamic)execution.Context.Message);
        }
    }
}
