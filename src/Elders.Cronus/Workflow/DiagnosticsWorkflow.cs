using System;
using System.Diagnostics;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Workflow
{
    public class DiagnosticsWorkflow<TContext> : Workflow<TContext> where TContext : HandlerContext
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
                logger.Info(() => "{cronus_MessageHandler} handled {cronus_MessageName} in {cronus_Elapsed}ms. => {cronus_MessageHeaders}", execution.Context.HandlerInstance.GetType().Name, execution.Context.Message.GetType().Name, elapsed.TotalMilliseconds, execution.Context.CronusMessage.Headers);
            }
        }
    }
}
