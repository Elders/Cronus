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

        public async Task<Exception> ToExceptionAsync()
        {
            string errorMessage = $"MessageHandleWorkflow execution has failed.{Environment.NewLine}{await GetMessageAsJsonAsync(ServiceProvider, Message).ConfigureAwait(false)}";
            return new WorkflowExecutionException(errorMessage, this, Error);
        }

        private async Task<string> GetMessageAsJsonAsync(IServiceProvider serviceProvider, CronusMessage message)
        {
            if (string.IsNullOrEmpty(MessageAsJson) && ServiceProvider is not null)
            {
                try
                {
                    var serializer = serviceProvider.GetRequiredService<ISerializer>();
                    using (var stream = new MemoryStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        serializer.Serialize(stream, message);
                        stream.Position = 0;
                        MessageAsJson = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception) { }
            }

            return MessageAsJson;
        }
    }
}
