using System;

namespace Elders.Cronus.MessageProcessing
{
    public interface IWorkflowContextWithServiceProvider
    {
        IServiceProvider ServiceProvider { get; set; }
    }

    public class ErrorContext : IWorkflowContextWithServiceProvider
    {
        public ErrorContext(Exception error, CronusMessage message, Type handlerType)
        {
            Error = error;
            Message = message;
            HandlerType = handlerType;
        }

        public Exception Error { get; private set; }

        public CronusMessage Message { get; private set; }

        public Type HandlerType { get; private set; }

        public IServiceProvider ServiceProvider { get; set; }
    }
}
