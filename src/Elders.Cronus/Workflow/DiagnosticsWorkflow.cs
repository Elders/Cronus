using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Workflow
{
    public class DiagnosticsWorkflow<TContext> : Workflow<TContext> where TContext : class
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(DiagnosticsWorkflow<>));
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        readonly Workflow<TContext> workflow;

        public DiagnosticsWorkflow(Workflow<TContext> workflow)
        {
            this.workflow = workflow;
        }

        protected override void Run(Execution<TContext> execution)
        {
            if (execution is null) throw new ArgumentNullException(nameof(execution));

            long startTimestamp = 0;
            if (logger.IsInfoEnabled())
                startTimestamp = Stopwatch.GetTimestamp();

            workflow.Run(execution.Context);

            if (logger.IsInfoEnabled())
            {
                var elapsed = new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - startTimestamp)));
                logger.Info($"{workflow.GetType().Name}: Executed handler {execution.Context.ToString()} in {elapsed.TotalMilliseconds}ms");
            }
        }
    }
}
