﻿using System;
using System.Runtime.Serialization;
using Elders.Cronus.Cluster.Job;
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
        private readonly RebuildIndex_ProjectionIndex_JobFactory jobFactory;

        public ProjectionBuilder(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, ICronusJobRunner jobRunner, RebuildIndex_ProjectionIndex_JobFactory jobFactory)
            : base(commandPublisher, timeoutRequestPublisher)
        {
            this.jobRunner = jobRunner;
            this.jobFactory = jobFactory;
        }

        public void Handle(ProjectionVersionRequested @event)
        {
            var startRebuildAt = @event.Timebox.RequestStartAt;
            if (startRebuildAt.AddMinutes(5) > DateTime.UtcNow && @event.Timebox.HasExpired == false)
            {
                RequestTimeout(new CreateNewProjectionVersion(@event, @event.Timebox.RequestStartAt));
                RequestTimeout(new ProjectionVersionRequestHeartbeat(@event, @event.Timebox.FinishRequestUntil));
            }
        }

        public void Handle(CreateNewProjectionVersion sagaTimeout)
        {
            RebuildIndex_ProjectionIndex_Job job = jobFactory.CreateJob(sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
            JobExecutionStatus result = jobRunner.ExecuteAsync(job).GetAwaiter().GetResult();
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

        public void Handle(ProjectionVersionRequestHeartbeat sagaTimeout)
        {
            var timedout = new TimeoutProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
            commandPublisher.Publish(timedout);
        }
    }

    [DataContract(Name = "029602fa-db90-47a4-9c8b-c304d5ee177a")]
    public class CreateNewProjectionVersion : ISystemScheduledMessage
    {
        CreateNewProjectionVersion() { }

        public CreateNewProjectionVersion(ProjectionVersionRequested projectionVersionRequest, DateTime publishAt)
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
    public class ProjectionVersionRequestHeartbeat : ISystemScheduledMessage
    {
        ProjectionVersionRequestHeartbeat() { }

        public ProjectionVersionRequestHeartbeat(ProjectionVersionRequested projectionVersionRequest, DateTime publishAt)
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
