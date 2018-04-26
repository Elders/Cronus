using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "d0dc548e-cbb1-4cb8-861b-e5f6bef68116")]
    public class ProjectionBuilder : Saga,
        IEventHandler<ProjectionVersionRequested>,
        ISagaTimeoutHandler<RebuildProjectionVersion>,
        ISagaTimeoutHandler<ProjectionVersionRebuildTimedout>
    {
        public ProjectionPlayer Player { get; set; }

        public void Handle(ProjectionVersionRequested @event)
        {
            var startRebuildAt = @event.Timebox.RebuildStartAt;
            if (startRebuildAt.AddMinutes(5) > DateTime.UtcNow)
            {
                RequestTimeout(new ProjectionVersionRebuildTimedout(@event, @event.Timebox.RebuildFinishUntil));
                RequestTimeout(new RebuildProjectionVersion(@event, @event.Timebox.RebuildStartAt));
            }
        }

        public void Handle(RebuildProjectionVersion @event)
        {
            if (Player.RebuildIndex() == false)
            {
                RequestTimeout(new RebuildProjectionVersion(@event.ProjectionVersionRequest, DateTime.UtcNow.AddSeconds(30)));
                return;
            }
            var rebuildUntil = @event.ProjectionVersionRequest.Timebox.RebuildFinishUntil;
            if (rebuildUntil < DateTime.UtcNow)
                return;

            var theType = @event.ProjectionVersionRequest.Version.ProjectionName.GetTypeByContract();
            var rebuildTimesOutAt = @event.ProjectionVersionRequest.Timebox.RebuildFinishUntil;
            if (Player.Rebuild(theType, @event.ProjectionVersionRequest.Version, rebuildTimesOutAt))
            {
                var command = new FinalizeProjectionVersionRequest(@event.ProjectionVersionRequest.Id, @event.ProjectionVersionRequest.Version);
                CommandPublisher.Publish(command);
            }
        }

        public void Handle(ProjectionVersionRebuildTimedout sagaTimeout)
        {
            var timedout = new TimeoutProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
            CommandPublisher.Publish(timedout);
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
    }
}
