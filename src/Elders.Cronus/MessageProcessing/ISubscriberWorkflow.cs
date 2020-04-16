using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    public interface ISubscriberWorkflow<T>
    {
        IWorkflow GetWorkflow();
    }
}
