using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    /// <summary>
    /// Work-flow which gets an object from the passed context and calls a method 'Index' with parameter '<see cref="CronusMessage"/>'
    /// <see cref="HandlerContext"/> should have 'HandlerInstance' and 'CronusMessage' already set
    /// </summary>
    public class DynamicMessageIndex : Workflow<HandlerContext>
    {
        protected override void Run(Execution<HandlerContext> execution)
        {
            dynamic handler = execution.Context.HandlerInstance;
            handler.Index((dynamic)execution.Context.CronusMessage);
        }
    }
}
