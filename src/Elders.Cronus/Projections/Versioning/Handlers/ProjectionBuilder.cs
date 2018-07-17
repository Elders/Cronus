using System;
using System.Runtime.Serialization;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "d0dc548e-cbb1-4cb8-861b-e5f6bef68116")]
    public class ProjectionBuilder : Saga,
        IEventHandler<ProjectionVersionRequested>,
        ISagaTimeoutHandler<RebuildProjectionVersion>,
        ISagaTimeoutHandler<ProjectionVersionRebuildTimedout>
    {
        static ILog log = LogProvider.GetLogger(typeof(ProjectionBuilder));

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
            bool indexExists = Player.HasIndex();
            if (indexExists == false)
                RequestTimeout(new RebuildProjectionVersion(@event.ProjectionVersionRequest, DateTime.UtcNow.AddSeconds(30)));

            if (indexExists || Player.RebuildIndex())
            {
                var rebuildUntil = @event.ProjectionVersionRequest.Timebox.RebuildFinishUntil;
                if (rebuildUntil < DateTime.UtcNow)
                    return;

                var theType = @event.ProjectionVersionRequest.Version.ProjectionName.GetTypeByContract();
                var rebuildTimesOutAt = @event.ProjectionVersionRequest.Timebox.RebuildFinishUntil;
                ReplayResult replayResult = Player.Rebuild(@event.ProjectionVersionRequest.Version, rebuildTimesOutAt);
                if (replayResult.IsSuccess)
                {
                    var finalize = new FinalizeProjectionVersionRequest(@event.ProjectionVersionRequest.Id, @event.ProjectionVersionRequest.Version);
                    CommandPublisher.Publish(finalize);
                }
                else
                {
                    log.Error(() => replayResult.Error);
                    if (replayResult.IsTimeout)
                    {
                        var timedout = new TimeoutProjectionVersionRequest(@event.ProjectionVersionRequest.Id, @event.ProjectionVersionRequest.Version, @event.ProjectionVersionRequest.Timebox);
                        CommandPublisher.Publish(timedout);
                    }
                    {
                        var cancel = new CancelProjectionVersionRequest(@event.ProjectionVersionRequest.Id, @event.ProjectionVersionRequest.Version, replayResult.Error);
                        CommandPublisher.Publish(cancel);
                    }
                }
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
