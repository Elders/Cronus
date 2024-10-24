using System.Threading.Tasks;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.MessageProcessing;

public sealed class LogExceptionOnHandleError : Workflow<ErrorContext>
{
    private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(LogExceptionOnHandleError));

    protected override Task RunAsync(Execution<ErrorContext> execution)
    {
        logger.LogError(execution.Context.Error, "There was an error in {cronus_MessageHandler} while handling message {@cronus_Message}", execution.Context.HandlerType.Name, execution.Context.Message);

        return Task.CompletedTask;
    }
}
