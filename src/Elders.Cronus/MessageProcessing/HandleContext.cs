using System;

namespace Elders.Cronus.MessageProcessing
{
    public class HandleContext : IWorkflowContextWithServiceProvider
    {
        public HandleContext(CronusMessage message, Type handlerType)
        {
            Message = message;
            HandlerType = handlerType;
        }
        public CronusMessage Message { get; private set; }

        public Type HandlerType { get; private set; }

        public IServiceProvider ServiceProvider { get; set; }
    }
}
