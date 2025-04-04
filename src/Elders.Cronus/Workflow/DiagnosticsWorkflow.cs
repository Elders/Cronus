﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Workflow;

public static class LogOption
{
    public static LogDefineOptions SkipLogInfoChecks = new LogDefineOptions() { SkipEnabledCheck = true };
}

internal static class CronusLogEvent
{
    public static EventId CronusHost = new EventId(74000, "CronusHost");
    public static EventId CronusWorkflowHandle = new EventId(74001, "CronusWorkflowHandle");
    public static EventId CronusEventStoreRead = new EventId(74010, "CronusEventStoreRead");
    public static EventId CronusEventStoreWrite = new EventId(74011, "CronusEventStoreWrite");
    public static EventId CronusProjectionRead = new EventId(74020, "CronusProjectionRead");
    public static EventId CronusProjectionWrite = new EventId(74021, "CronusProjectionWrite");
    public static EventId CronusJobOk = new EventId(74100, "CronusJobOk");
    public static EventId CronusJobError = new EventId(74101, "CronusJobError");
    public static EventId CronusPublishOk = new EventId(74200, "CronusPublishOk");
    public static EventId CronusPublishError = new EventId(74201, "CronusPublishError");
}

public sealed class DiagnosticsWorkflow<TContext> : Workflow<TContext> where TContext : HandleContext
{
    private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(DiagnosticsWorkflow<>));
    private static readonly Action<ILogger, string, string, double, Exception> LogHandleSuccess = LoggerMessage.Define<string, string, double>(LogLevel.Information, CronusLogEvent.CronusWorkflowHandle, "{cronus_MessageHandler} handled {cronus_MessageType} in {ElapsedMilliseconds:0.0000}ms.", LogOption.SkipLogInfoChecks);
    private static readonly Action<ILogger, string, string, Exception> LogHandleStarting = LoggerMessage.Define<string, string>(LogLevel.Debug, CronusLogEvent.CronusWorkflowHandle, "{cronus_MessageHandler} starting handle {cronus_MessageType}.", LogOption.SkipLogInfoChecks);

    private const string ActivityName = "Elders.Cronus.Hosting.Workflow";
    private const string DiagnosticsUnhandledExceptionKey = "Elders.Cronus.Hosting.UnhandledException";

    readonly Workflow<TContext> workflow;
    private readonly DiagnosticListener diagnosticListener;
    private readonly ActivitySource activitySource;

    public DiagnosticsWorkflow(Workflow<TContext> workflow, DiagnosticListener diagnosticListener, ActivitySource activitySource)
    {
        this.workflow = workflow;
        this.diagnosticListener = diagnosticListener;
        this.activitySource = activitySource;
    }

    protected override async Task RunAsync(Execution<TContext> execution)
    {
        if (execution is null) throw new ArgumentNullException(nameof(execution));

        using (logger.BeginScope(scope =>
        {
            string tenant = execution.Context.Message.GetTenant();
            scope.AddScope(Log.Tenant, tenant);

            if (execution.Context.Message.TryGetRootId(out string rootId))
                scope.AddScope(Log.AggregateId, rootId);
        }))
        {
            Activity activity = StartActivity(execution.Context);

            Type msgType = execution.Context.Message.Payload.GetType();

            LogHandleStarting(logger, execution.Context.HandlerType.Name, msgType.Name, null);
            if (logger.IsEnabled(LogLevel.Information))
            {
                long startTimestamp = 0;
                startTimestamp = Stopwatch.GetTimestamp();

                await workflow.RunAsync(execution.Context).ConfigureAwait(false);

                TimeSpan elapsed = Stopwatch.GetElapsedTime(startTimestamp);

                LogHandleSuccess(logger, execution.Context.HandlerType.Name, msgType.Name, elapsed.TotalMilliseconds, null);
            }
            else
            {
                await workflow.RunAsync(execution.Context).ConfigureAwait(false);
            }

            StopActivity(activity);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Activity StartActivity(TContext context)
    {
        if (diagnosticListener.IsEnabled())
        {
            Activity activity = null;
            string parentId = string.Empty;
            context.Message.Headers.TryGetValue("telemetry_traceparent", out parentId);
            string activityName = $"{context.HandlerType.Name}__{context.Message.Payload.GetType().Name}";
            if (ActivityContext.TryParse(parentId, null, out ActivityContext ctx))
            {
                activity = activitySource.CreateActivity(activityName, ActivityKind.Server, ctx);
            }
            else
            {
                activity = activitySource.CreateActivity(activityName, ActivityKind.Server, parentId);
            }

            if (activity is null)
            {
                activity = new Activity(activityName);

                if (string.IsNullOrEmpty(parentId) == false)
                    activity.SetParentId(parentId);
            }

            activity.SetTag(Log.MessageId, context.Message.Id.ToString());
            activity.Start();

            return activity;
        }

        return null;
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
