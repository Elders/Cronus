using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore.Players;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections.Rebuilding;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.Projections.Versioning;

[DataContract(Name = "d0dc548e-cbb1-4cb8-861b-e5f6bef68116")]
public sealed class ProjectionBuilder : Saga, ISystemSaga,
    IEventHandler<ProjectionVersionRequested>,
    IEventHandler<ProjectionVersionRequestPaused>,
    ISagaTimeoutHandler<CreateNewProjectionVersion>,
    ISagaTimeoutHandler<ProjectionVersionRequestHeartbeat>
{
    private static readonly Action<ILogger, JobExecutionStatus, Exception> LogProjectionReplayStatus = LoggerMessage.Define<JobExecutionStatus>(LogLevel.Debug, CronusLogEvent.CronusProjectionWrite, "Replay projection version {@cronus_projection_rebuild}.");

    private ILogger logger;

    private TenantsOptions tenants;
    private readonly ICronusJobRunner jobRunner;
    private readonly RebuildProjection_JobFactory fastJobFactory;
    private readonly RebuildProjectionSequentially_JobFactory sequentialJobFactory;

    public ProjectionBuilder(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, IOptionsMonitor<TenantsOptions> monitor, ICronusJobRunner jobRunner, RebuildProjection_JobFactory fastJobFactory, RebuildProjectionSequentially_JobFactory sequentialJobFactory, ILogger<ProjectionBuilder> logger)
        : base(commandPublisher, timeoutRequestPublisher)
    {
        this.tenants = monitor.CurrentValue;
        this.jobRunner = jobRunner;
        this.fastJobFactory = fastJobFactory;
        this.sequentialJobFactory = sequentialJobFactory;
        this.logger = logger;

        monitor.OnChange(OptionsForTenantReloaded);
    }

    /// <summary>
    /// Schedules the saga timeout that drives the projection-rebuild loop.
    /// </summary>
    /// <param name="event">The event signalling that a new projection version was requested.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(ProjectionVersionRequested @event, CancellationToken cancellationToken = default)
    {
        var startRebuildAt = @event.Timebox.RequestStartAt;
        if (startRebuildAt.AddMinutes(5) > DateTime.UtcNow && @event.Timebox.HasExpired == false)
        {
            await RequestTimeoutAsync(new CreateNewProjectionVersion(@event, @event.Timebox.RequestStartAt), cancellationToken).ConfigureAwait(false);
            //await RequestTimeoutAsync(new ProjectionVersionRequestHeartbeat(@event, @event.Timebox.FinishRequestUntil)).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Cancels the rebuild job for a projection version that has been paused.
    /// </summary>
    /// <param name="event">The event signalling that the projection version was paused.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(ProjectionVersionRequestPaused @event, CancellationToken cancellationToken = default)
    {
        var job = GetJob(@event.Version, new ReplayEventsOptions(), new VersionRequestTimebox(@event.Timestamp.DateTime));

        return jobRunner.JobManager.CancelAsync(job.Name);
    }

    private ICronusJob<object> GetJob(ProjectionVersion version, ReplayEventsOptions replayEventsOptions, VersionRequestTimebox requestTimebox)
    {
        IProjection_JobFactory factory;
        var projectionType = version.ProjectionName.GetTypeByContract();
        if (projectionType.IsAssignableTo(typeof(IAmEventSourcedProjectionFast)) || projectionType.IsAssignableTo(typeof(IProjectionDefinition)))
            factory = fastJobFactory;
        else
            factory = sequentialJobFactory;

        ICronusJob<object> job = factory.CreateJob(version, replayEventsOptions, requestTimebox);

        return job;
    }

    /// <summary>
    /// Drives a slice of the projection-rebuild job and either re-arms the saga, fails it, or finalises the version request based on the job result.
    /// </summary>
    /// <param name="sagaTimeout">The saga timeout that fired.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(CreateNewProjectionVersion sagaTimeout, CancellationToken cancellationToken = default)
    {
        if (tenants.Tenants.Contains(sagaTimeout.Tenant) == false)
        {
            logger.LogWarning("Tenant is not present in the tenants configuration, and the projection won't be rebuilt.");
            return;
        }

        ICronusJob<object> job = GetJob(sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.ReplayEventsOptions, sagaTimeout.ProjectionVersionRequest.Timebox);
        JobExecutionStatus result = await jobRunner.ExecuteAsync(job, cancellationToken).ConfigureAwait(false);
        LogProjectionReplayStatus(logger, result, null);

        if (result == JobExecutionStatus.Running)
        {
            await RequestTimeoutAsync(new CreateNewProjectionVersion(sagaTimeout.ProjectionVersionRequest, DateTime.UtcNow.AddSeconds(60)), cancellationToken).ConfigureAwait(false);
        }
        else if (result == JobExecutionStatus.Failed)
        {
            var cancel = new CancelProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, "Failed");
            await commandPublisher.PublishAsync(cancel, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else if (result == JobExecutionStatus.Completed)
        {
            var finalize = new FinalizeProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version);
            await commandPublisher.PublishAsync(finalize, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Publishes a timeout command when the projection rebuild has not finished within its allotted timebox.
    /// </summary>
    /// <param name="sagaTimeout">The heartbeat saga timeout that fired.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(ProjectionVersionRequestHeartbeat sagaTimeout, CancellationToken cancellationToken = default)
    {
        var timedout = new TimeoutProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
        await commandPublisher.PublishAsync(timedout, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private void OptionsForTenantReloaded(TenantsOptions newOptions)
    {
        if (tenants.Tenants.SequenceEqual(newOptions.Tenants) == false)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Cronus tenants options re-loaded with {@options}", newOptions);

            tenants = newOptions;
        }
    }
}

[DataContract(Name = "029602fa-db90-47a4-9c8b-c304d5ee177a")]
public sealed class CreateNewProjectionVersion : ISystemScheduledMessage
{
    CreateNewProjectionVersion()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    public CreateNewProjectionVersion(ProjectionVersionRequested projectionVersionRequest, DateTime publishAt) : this()
    {
        ProjectionVersionRequest = projectionVersionRequest;
        PublishAt = publishAt;
    }

    [DataMember(Order = 1)]
    public ProjectionVersionRequested ProjectionVersionRequest { get; private set; }

    [DataMember(Order = 2)]
    public DateTime PublishAt { get; set; }

    [DataMember(Order = 3)]
    public DateTimeOffset Timestamp { get; private set; }

    public string Tenant { get { return ProjectionVersionRequest.Id.Tenant; } }
}

[DataContract(Name = "11c1ae7d-04f4-4266-a21e-78ddc440d68b")]
public sealed class ProjectionVersionRequestHeartbeat : ISystemScheduledMessage
{
    ProjectionVersionRequestHeartbeat()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    public ProjectionVersionRequestHeartbeat(ProjectionVersionRequested projectionVersionRequest, DateTime publishAt) : this()
    {
        ProjectionVersionRequest = projectionVersionRequest;
        PublishAt = publishAt;
    }

    [DataMember(Order = 1)]
    public ProjectionVersionRequested ProjectionVersionRequest { get; private set; }

    [DataMember(Order = 2)]
    public DateTime PublishAt { get; set; }

    [DataMember(Order = 2)]
    public DateTimeOffset Timestamp { get; private set; }

    public string Tenant { get { return ProjectionVersionRequest.Id.Tenant; } }
}
