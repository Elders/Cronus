using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Workflow
{
    public class DiagnosticsWorkflow<TContext> : Workflow<TContext> where TContext : HandleContext
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(DiagnosticsWorkflow<>));
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private const string ActivityName = "Elders.Cronus.Hosting.Workflow";
        private const string DiagnosticsUnhandledExceptionKey = "Elders.Cronus.Hosting.UnhandledException";

        readonly Workflow<TContext> workflow;
        private readonly DiagnosticListener diagnosticListener;

        public DiagnosticsWorkflow(Workflow<TContext> workflow, DiagnosticListener diagnosticListener)
        {
            this.workflow = workflow;
            this.diagnosticListener = diagnosticListener;
        }

        protected override async Task RunAsync(Execution<TContext> execution)
        {
            if (execution is null) throw new ArgumentNullException(nameof(execution));

            Activity activity = null;
            if (diagnosticListener.IsEnabled())
            {
                activity = new Activity($"{execution.Context.HandlerType.Name}__{execution.Context.Message.Payload.GetType().Name}");
                activity.Start();
            }

            Type msgType = execution.Context.Message.Payload.GetType();

            if (logger.IsInfoEnabled())
            {
                string scopeId = GetScopeId(execution.Context.Message);

                using (logger.BeginScope(scopeId))
                {
                    long startTimestamp = 0;
                    startTimestamp = Stopwatch.GetTimestamp();

                    await workflow.RunAsync(execution.Context).ConfigureAwait(false);

                    TimeSpan elapsed = new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - startTimestamp)));
                    logger.Info(() => "{cronus_MessageHandler} handled {cronus_MessageName} in {Elapsed:0.0000} ms", execution.Context.HandlerType.Name, msgType.Name, elapsed.TotalMilliseconds, execution.Context.Message.Headers);
                }
            }
            else
            {
                await workflow.RunAsync(execution.Context).ConfigureAwait(false);
            }

            StopActivity(activity);
        }

        private string GetScopeId(CronusMessage cronusMessage)
        {
            if (cronusMessage.Headers.TryGetValue(MessageHeader.CorelationId, out string scopeId) == false)
            {
                scopeId = Guid.NewGuid().ToString();
            }

            return scopeId;
        }

        private void StopActivity(Activity activity)
        {
            if (activity is null) return;
            // Stop sets the end time if it was unset, but we want it set before we issue the write
            // so we do it now.
            if (activity.Duration == TimeSpan.Zero)
            {
                activity.SetEndTime(DateTime.UtcNow);
            }
            diagnosticListener.Write(ActivityName, activity);
            activity.Stop();    // Resets Activity.Current (we want this after the Write)
        }
    }

    public sealed class ExceptionEaterWorkflow<TContext> : Workflow<TContext> where TContext : HandleContext
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(DiagnosticsWorkflow<>));

        readonly Workflow<TContext> workflow;

        public ExceptionEaterWorkflow(Workflow<TContext> workflow)
        {
            this.workflow = workflow;
        }
        protected override async Task RunAsync(Execution<TContext> execution)
        {
            try { await workflow.RunAsync(execution.Context); } // here we shouldn't elide async kwyword 'cause it'll raise an exception outside this catch
            catch (Exception ex) when (logger.ErrorException(ex, () => "Somewhere along the way an exception was thrown and it was eaten. See inner exception")) { }
        }
    }
}
