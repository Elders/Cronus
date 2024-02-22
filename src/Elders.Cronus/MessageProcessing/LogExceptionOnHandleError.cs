using System.Threading.Tasks;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.MessageProcessing;

public class LogExceptionOnHandleError : Workflow<ErrorContext>
{
    private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(LogExceptionOnHandleError));

    protected override Task RunAsync(Execution<ErrorContext> execution)
    {
        var serializer = execution.Context.ServiceProvider.GetRequiredService<ISerializer>();
        logger.ErrorException(execution.Context.Error, () => $"There was an error in {execution.Context.HandlerType.Name} while handling message {serializer.SerializeToString(execution.Context.Message)}");

        return Task.CompletedTask;
    }
}
