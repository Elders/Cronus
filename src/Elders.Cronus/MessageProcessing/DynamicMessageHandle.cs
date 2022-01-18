using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

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

    public class LogExceptionOnHandleError : Workflow<ErrorContext>
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(LogExceptionOnHandleError));
        protected override void Run(Execution<ErrorContext> execution)
        {
            var serializer = execution.Context.ServiceProvider.GetRequiredService<ISerializer>();

            logger.ErrorException(execution.Context.Error, () => $"There was an error in {execution.Context.HandlerType.Name} while handling message {MessageAsString(serializer, execution.Context.Message)}");
        }

        private string MessageAsString(ISerializer serializer, CronusMessage message)
        {
            using (var stream = new MemoryStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                serializer.Serialize(stream, message);
                stream.Position = 0;
                return reader.ReadToEnd();
            }
        }
    }
}
