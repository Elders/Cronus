using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    /// <summary>
    /// Work-flow which gets an object from the passed context and calls a method 'Handle' passing execution.Context.Message'
    /// <see cref="HandlerContext"/> should have 'HandlerInstance' and 'Message' already set
    /// </summary>
    public class DynamicMessageHandle : Workflow<HandlerContext>
    {
        protected override void Run(Execution<HandlerContext> execution)
        {
            dynamic handler = execution.Context.HandlerInstance;
            handler.Handle((dynamic)execution.Context.Message);
        }
    }
}
