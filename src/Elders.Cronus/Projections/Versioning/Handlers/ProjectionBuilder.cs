using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "d0dc548e-cbb1-4cb8-861b-e5f6bef68116")]
    public class ProjectionBuilder : Saga,
        IEventHandler<ProjectionVersionRequested>,
        ISagaTimeoutHandler<RebuildProjectionVersion>,
        ISagaTimeoutHandler<ProjectionVersionRebuildTimedout>
    {
        private ILogger logger = CronusLogger.CreateLogger(typeof(ProjectionBuilder));

        private readonly ProjectionPlayer projectionPlayer;

        public ProjectionBuilder(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, ProjectionPlayer projectionPlayer)
            : base(commandPublisher, timeoutRequestPublisher)
        {
            this.projectionPlayer = projectionPlayer;
        }

        public void Handle(ProjectionVersionRequested @event)
        {
            var startRebuildAt = @event.Timebox.RebuildStartAt;
            if (startRebuildAt.AddMinutes(5) > DateTime.UtcNow && @event.Timebox.HasExpired == false)
            {
                RequestTimeout(new RebuildProjectionVersion(@event, @event.Timebox.RebuildStartAt));
                RequestTimeout(new ProjectionVersionRebuildTimedout(@event, @event.Timebox.RebuildFinishUntil));
            }
        }

        public void Handle(RebuildProjectionVersion @event)
        {
            ReplayResult result = projectionPlayer.Rebuild(@event.ProjectionVersionRequest.Version, @event.ProjectionVersionRequest.Timebox.RebuildFinishUntil);

            HandleRebuildResult(result, @event);
        }

        void HandleRebuildResult(ReplayResult result, RebuildProjectionVersion @event)
        {
            if (result.IsSuccess)
            {
                var finalize = new FinalizeProjectionVersionRequest(@event.ProjectionVersionRequest.Id, @event.ProjectionVersionRequest.Version);
                commandPublisher.Publish(finalize);
            }
            else if (result.ShouldRetry)
            {
                RequestTimeout(new RebuildProjectionVersion(@event.ProjectionVersionRequest, DateTime.UtcNow.AddSeconds(30)));
            }
            else
            {
                logger.Error(() => result.Error);
                if (result.IsTimeout)
                {
                    var timedout = new TimeoutProjectionVersionRequest(@event.ProjectionVersionRequest.Id, @event.ProjectionVersionRequest.Version, @event.ProjectionVersionRequest.Timebox);
                    commandPublisher.Publish(timedout);
                }
                else
                {
                    var cancel = new CancelProjectionVersionRequest(@event.ProjectionVersionRequest.Id, @event.ProjectionVersionRequest.Version, result.Error);
                    commandPublisher.Publish(cancel);
                }
            }
        }

        public void Handle(ProjectionVersionRebuildTimedout sagaTimeout)
        {
            var timedout = new TimeoutProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
            commandPublisher.Publish(timedout);
        }
    }

    [DataContract(Name = "029602fa-db90-47a4-9c8b-c304d5ee177a")]
    public class RebuildProjectionVersion : IScheduledMessage
    {
        RebuildProjectionVersion() { }

        public RebuildProjectionVersion(ProjectionVersionRequested projectionVersionRequest, DateTime publishAt)
        {
            ProjectionVersionRequest = projectionVersionRequest;
            PublishAt = publishAt;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionRequested ProjectionVersionRequest { get; private set; }

        [DataMember(Order = 2)]
        public DateTime PublishAt { get; set; }

        public string Tenant { get { return ProjectionVersionRequest.Id.Tenant; } }
    }

    [DataContract(Name = "11c1ae7d-04f4-4266-a21e-78ddc440d68b")]
    public class ProjectionVersionRebuildTimedout : IScheduledMessage
    {
        ProjectionVersionRebuildTimedout() { }

        public ProjectionVersionRebuildTimedout(ProjectionVersionRequested projectionVersionRequest, DateTime publishAt)
        {
            ProjectionVersionRequest = projectionVersionRequest;
            PublishAt = publishAt;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionRequested ProjectionVersionRequest { get; private set; }

        [DataMember(Order = 2)]
        public DateTime PublishAt { get; set; }

        public string Tenant { get { return ProjectionVersionRequest.Id.Tenant; } }
    }
}
