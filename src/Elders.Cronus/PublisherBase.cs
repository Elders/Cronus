using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elders.Cronus;

public abstract class PublisherHandler
{
    protected bool handledByRealPublisher = false;

    protected internal virtual bool PublishInternal(CronusMessage message)
    {
        throw new NotImplementedException();
    }
}

public abstract class DelegatingPublishHandler : PublisherHandler
{
    protected internal override bool PublishInternal(CronusMessage message)
    {
        if (InnerHandler is null)
            throw new InvalidOperationException("The inner publisher handler is not set.");

        return InnerHandler.PublishInternal(message);
    }

    internal PublisherHandler InnerHandler { get; set; }
}

internal class CronusHeadersPublishHandler : DelegatingPublishHandler
{
    private readonly ITenantResolver<IMessage> tenantResolver;
    private readonly BoundedContext boundedContext;

    public CronusHeadersPublishHandler(ITenantResolver<IMessage> tenantResolver, IOptions<BoundedContext> boundedContextOptions)
    {
        this.tenantResolver = tenantResolver;
        this.boundedContext = boundedContextOptions.Value;
    }

    protected internal override bool PublishInternal(CronusMessage message)
    {
        Type payloadType = message.GetMessageType();

        if (message.Headers.ContainsKey(MessageHeader.PublishTimestamp) == false)
            message.Headers.Add(MessageHeader.PublishTimestamp, DateTime.UtcNow.ToFileTimeUtc().ToString());

        if (message.Headers.ContainsKey(MessageHeader.Tenant) == false)
            message.Headers.Add(MessageHeader.Tenant, tenantResolver.Resolve(message.Payload));

        if (message.Headers.ContainsKey(MessageHeader.BoundedContext))
        {
            var bc = payloadType.GetBoundedContext(boundedContext.Name);
            message.Headers[MessageHeader.BoundedContext] = bc;
        }
        else
        {
            var bc = payloadType.GetBoundedContext(boundedContext.Name);
            message.Headers.Add(MessageHeader.BoundedContext, bc);
        }

        message.Headers.Remove("contract_name");
        message.Headers.Add("contract_name", payloadType.GetContractId());

        return base.PublishInternal(message);
    }
}

internal class LoggingPublishHandler : DelegatingPublishHandler
{
    private readonly ILogger<LoggingPublishHandler> logger;

    public LoggingPublishHandler(ILogger<LoggingPublishHandler> logger)
    {
        this.logger = logger;
    }

    protected internal override bool PublishInternal(CronusMessage message)
    {
        using (logger.BeginScope(s => s.AddScope("cronus_messageid", message.Id.ToString())))
        {
            try
            {
                bool isPublished = base.PublishInternal(message) && handledByRealPublisher;

                Type messageType = message.GetMessageType();

                bool isSignal = messageType.IsAssignableFrom(typeof(ISystemSignal));
                if (isPublished && isSignal == false)
                {
                    logger.Info(() => "Publish {cronus_MessageType} {cronus_MessageName} - OK", messageType.Name, messageType.Name);
                }
                else if (isPublished == false)
                {
                    logger.Error(() => "Publish {cronus_MessageType} {cronus_MessageName} - Fail", messageType.Name, messageType.Name);
                }

                return isPublished;
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => BuildTraceData()))
            {
                return false;
            }
        }

        string BuildTraceData()
        {
            StringBuilder errorMessage = new StringBuilder();
            errorMessage.AppendLine("Failed to publish message!");

            errorMessage.AppendLine("Headers:");
            foreach (var header in message.Headers)
            {
                errorMessage.AppendLine($"{header.Key}:{header.Value}");
            }

            string messageString = JsonSerializer.Serialize<object>(message);
            errorMessage.AppendLine(messageString);

            return errorMessage.ToString();
        }
    }
}

internal class ActivityPublishHandler : DelegatingPublishHandler
{
    private const string TelemetryTraceParent = "telemetry_traceparent";
    private readonly DiagnosticListener diagnosticListener;
    private readonly ActivitySource activitySource;
    private readonly ILogger<ActivityPublishHandler> logger;

    public ActivityPublishHandler(DiagnosticListener diagnosticListener, ActivitySource activitySource, ILogger<ActivityPublishHandler> logger)
    {
        this.diagnosticListener = diagnosticListener;
        this.activitySource = activitySource;
        this.logger = logger;
    }

    protected internal override bool PublishInternal(CronusMessage message)
    {
        Activity activity = StartActivity(message);
        if (Activity.Current is not null)
        {
            message.Headers.Remove(TelemetryTraceParent);
            message.Headers.Add(TelemetryTraceParent, Activity.Current.Id);
        }

        bool published = base.PublishInternal(message);
        StopActivity(activity);

        return published;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Activity StartActivity(CronusMessage message)
    {
        var asd = Activity.Current;
        if (diagnosticListener.IsEnabled())
        {
            Activity activity = null;
            string parentId = string.Empty;
            message.Headers?.TryGetValue(TelemetryTraceParent, out parentId);
            string messageTypeName = message.GetMessageType().Name;
            string activityName = $"Publish {messageTypeName}";
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
                if (!string.IsNullOrEmpty(parentId))
                {
                    activity.SetParentId(parentId);
                }
            }

            activity.SetTag("cronus_messageid", message.Id.ToString());

            activity.Start();

            return activity;
        }

        return null;
    }

    private void StopActivity(Activity activity)
    {
        if (activity is null) return;

        try
        {
            // Stop sets the end time if it was unset, but we want it set before we issue the write so we do it now.
            if (activity.Duration == TimeSpan.Zero)
                activity.SetEndTime(DateTime.UtcNow);


            diagnosticListener.Write(ActivityName, activity);
            activity.Stop();    // Resets Activity.Current (we want this after the Write)
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stopping activity failed.");
        }
    }

    private const string ActivityName = "Elders.Cronus.Hosting.Workflow";
}

public abstract class PublisherBase<TMessage> : PublisherHandler, IPublisher<TMessage> where TMessage : IMessage
{
    private readonly IEnumerable<DelegatingPublishHandler> handlers;

    public PublisherBase(IEnumerable<DelegatingPublishHandler> handlers)
    {
        this.handlers = handlers.Cast<DelegatingPublishHandler>();
    }

    public virtual bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
    {
        if (messageHeaders is null)
            messageHeaders = new Dictionary<string, string>();

        var cronusMessage = new CronusMessage(message, messageHeaders);

        var enumerator = handlers.GetEnumerator();
        bool hasHandlers = enumerator.MoveNext();
        if (hasHandlers == false)
            return PublishInternal(cronusMessage);

        while (hasHandlers)
        {
            DelegatingPublishHandler currentHandler = enumerator.Current;
            hasHandlers = enumerator.MoveNext();
            if (hasHandlers)
            {
                currentHandler.InnerHandler = enumerator.Current;
            }
            else
            {
                currentHandler.InnerHandler = this;
            }
        }

        return handlers.First().PublishInternal(cronusMessage);
    }

    public virtual bool Publish(byte[] messageRaw, Type messageType, string tenant, Dictionary<string, string> messageHeaders)
    {
        if (messageHeaders is null)
            messageHeaders = new Dictionary<string, string>();

        EnsureValidTenant(tenant, messageHeaders);

        if (typeof(TMessage).IsAssignableFrom(messageType) == false)
            throw new ArgumentException($"Publisher {this.GetType().Name} cannot publish a message of type {messageType.Name}");

        CronusMessage cronusMessage = new CronusMessage(messageRaw, messageType, messageHeaders);

        IEnumerator<DelegatingPublishHandler> enumerator = handlers.GetEnumerator();
        bool hasHandlers = enumerator.MoveNext();
        if (hasHandlers == false)
            return PublishInternal(cronusMessage);

        while (hasHandlers)
        {
            DelegatingPublishHandler currentHandler = enumerator.Current;
            hasHandlers = enumerator.MoveNext();
            if (hasHandlers)
            {
                currentHandler.InnerHandler = enumerator.Current;
            }
            else
            {
                currentHandler.InnerHandler = this;
            }
        }

        return handlers.First().PublishInternal(cronusMessage);
    }

    public virtual bool Publish(TMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null)
    {
        messageHeaders = messageHeaders ?? new Dictionary<string, string>();
        messageHeaders.Add(MessageHeader.PublishTimestamp, publishAt.ToFileTimeUtc().ToString());
        return Publish(message, messageHeaders);
    }

    public bool Publish(TMessage message, TimeSpan publishAfter, Dictionary<string, string> messageHeaders = null)
    {
        DateTime publishAt = DateTime.UtcNow.Add(publishAfter);
        return Publish(message, publishAt, messageHeaders);
    }

    private void EnsureValidTenant(string tenant, Dictionary<string, string> messageHeaders)
    {
        if (string.IsNullOrEmpty(tenant))
            throw new ArgumentNullException("Unable to publish a message without a specified tenant.");

        bool hasTenantHeader = messageHeaders.ContainsKey(MessageHeader.Tenant);
        if (hasTenantHeader)
        {
            if (tenant.Equals(messageHeaders[MessageHeader.Tenant], StringComparison.OrdinalIgnoreCase) == false)
                throw new ArgumentException("Unable to publish a message inconsistency between message headers and the tenant from input parameters.");
        }
        else
        {
            messageHeaders.Add(MessageHeader.Tenant, tenant);
        }
    }
}
