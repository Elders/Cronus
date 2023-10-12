using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Projections.Rebuilding;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "d0dc548e-cbb1-4cb8-861b-e5f6bef68116")]
    public class ProjectionBuilder : Saga, ISystemSaga,
        IEventHandler<ProjectionVersionRequested>,
        ISagaTimeoutHandler<CreateNewProjectionVersion>,
        ISagaTimeoutHandler<ProjectionVersionRequestHeartbeat>
    {
        private static ILogger logger = CronusLogger.CreateLogger(typeof(ProjectionBuilder));

        private readonly ICronusJobRunner jobRunner;
        private readonly Projection_JobFactory jobFactory;

        public ProjectionBuilder(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, ICronusJobRunner jobRunner, Projection_JobFactory jobFactory)
            : base(commandPublisher, timeoutRequestPublisher)
        {
            this.jobRunner = jobRunner;
            this.jobFactory = jobFactory;
        }

        public Task HandleAsync(ProjectionVersionRequested @event)
        {
            var startRebuildAt = @event.Timebox.RequestStartAt;
            if (startRebuildAt.AddMinutes(5) > DateTime.UtcNow && @event.Timebox.HasExpired == false)
            {
                RequestTimeout(new CreateNewProjectionVersion(@event, @event.Timebox.RequestStartAt));
                //RequestTimeout(new ProjectionVersionRequestHeartbeat(@event, @event.Timebox.FinishRequestUntil));
            }

            return Task.CompletedTask;
        }

        public async Task HandleAsync(CreateNewProjectionVersion sagaTimeout)
        {
            RebuildProjection_Job job = jobFactory.CreateJob(sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.ReplayEventsOptions, sagaTimeout.ProjectionVersionRequest.Timebox);
            JobExecutionStatus result = await jobRunner.ExecuteAsync(job).ConfigureAwait(false);
            logger.Debug(() => "Replay projection version {@cronus_projection_rebuild}", result);

            if (result == JobExecutionStatus.Running)
            {
                RequestTimeout(new CreateNewProjectionVersion(sagaTimeout.ProjectionVersionRequest, DateTime.UtcNow.AddSeconds(30)));
            }
            else if (result == JobExecutionStatus.Failed)
            {
                var cancel = new CancelProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, "Failed");
                commandPublisher.Publish(cancel);
            }
            else if (result == JobExecutionStatus.Completed)
            {
                var finalize = new FinalizeProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version);
                commandPublisher.Publish(finalize);
            }
        }

        public Task HandleAsync(ProjectionVersionRequestHeartbeat sagaTimeout)
        {
            var timedout = new TimeoutProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
            commandPublisher.Publish(timedout);

            return Task.CompletedTask;
        }
    }

    [DataContract(Namespace = "cronus", Name = "029602fa-db90-47a4-9c8b-c304d5ee177a")]
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

    [DataContract(Namespace = "cronus", Name = "11c1ae7d-04f4-4266-a21e-78ddc440d68b")]
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
}
