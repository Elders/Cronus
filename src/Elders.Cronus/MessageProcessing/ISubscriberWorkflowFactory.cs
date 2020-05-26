using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    public interface ISubscriberWorkflowFactory<T>
    {
        IWorkflow GetWorkflow();
    }
}
