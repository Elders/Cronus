using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

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

        public string MessageAsJson { get; private set; }

        public Exception ToException()
        {
            string errorMessage = $"MessageHandleWorkflow execution has failed.{Environment.NewLine}{GetMessageAsJson(ServiceProvider, Message)}";
            return new WorkflowExecutionException(errorMessage, this, Error);
        }

        private string GetMessageAsJson(IServiceProvider serviceProvider, CronusMessage message)
        {
            if (string.IsNullOrEmpty(MessageAsJson) && ServiceProvider is not null)
            {
                try
                {
                    var serializer = serviceProvider.GetRequiredService<ISerializer>();
                    MessageAsJson = serializer.SerializeToString(message);
                }
                catch (Exception) { }
            }

            return MessageAsJson;
        }
    }
}
