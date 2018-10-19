using System;
using System.Linq;
using System.Runtime.Serialization;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Logging;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionPlayerFactory
    {
        private readonly IEventStoreFactory eventStoreFactory;
        private readonly IProjectionStoreFactory projectionStoreFactory;
        private readonly IProjectionServicesFactory projectionServicesFactory;

        public ProjectionPlayerFactory(IEventStoreFactory eventStoreFactory, IProjectionStoreFactory projectionStoreFactory, IProjectionServicesFactory projectionServicesFactory)
        {
            this.eventStoreFactory = eventStoreFactory;
            this.projectionStoreFactory = projectionStoreFactory;
            this.projectionServicesFactory = projectionServicesFactory;
        }

        public ProjectionPlayer GetPlayerFor(string tenant)
        {
            IEventStore eventStore = eventStoreFactory.GetEventStore(tenant);
            IEventStorePlayer eventStorePlayer = eventStoreFactory.GetEventStorePlayer(tenant);
            EventStoreIndex index = eventStoreFactory.GetEventStoreIndex(tenant);
            IProjectionStore projectionStore = projectionStoreFactory.GetProjectionStore(tenant);
            IProjectionReader projectionReader = projectionServicesFactory.GetProjectionReader(tenant);
            IProjectionWriter projectionWriter = projectionServicesFactory.GetProjectionWriter(tenant);

            return new ProjectionPlayer(eventStore, eventStorePlayer, index, projectionWriter, projectionReader);
        }
    }


    [DataContract(Name = "d0dc548e-cbb1-4cb8-861b-e5f6bef68116")]
    public class ProjectionBuilder : Saga,
        IEventHandler<ProjectionVersionRequested>,
        ISagaTimeoutHandler<RebuildProjectionVersion>,
        ISagaTimeoutHandler<ProjectionVersionRebuildTimedout>
    {
        static ILog log = LogProvider.GetLogger(typeof(ProjectionBuilder));

        private readonly ProjectionPlayerFactory projectionPlayerFactory;

        public ProjectionBuilder(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, ProjectionPlayerFactory projectionPlayerFactory)
            : base(commandPublisher, timeoutRequestPublisher)
        {
            this.projectionPlayerFactory = projectionPlayerFactory;
        }

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
            ReplayResult result = projectionPlayerFactory
                .GetPlayerFor(@event.ProjectionVersionRequest.Tenant)
                .Rebuild(@event.ProjectionVersionRequest.Version, @event.ProjectionVersionRequest.Timebox.RebuildFinishUntil);

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
                log.Error(() => result.Error);
                if (result.IsTimeout)
                {
                    var timedout = new TimeoutProjectionVersionRequest(@event.ProjectionVersionRequest.Id, @event.ProjectionVersionRequest.Version, @event.ProjectionVersionRequest.Timebox);
                    commandPublisher.Publish(timedout);
                }
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

    public class SystemProjectionsGG
    {
        private readonly TypeContainer<IProjection> handlerTypeContainer;
        private readonly IProjectionReader projectionReader;

        public SystemProjectionsGG(TypeContainer<IProjection> handlerTypeContainer, IProjectionReader projectionReader)
        {
            this.handlerTypeContainer = handlerTypeContainer;
            this.projectionReader = projectionReader;
        }

        public bool AreWeOK()
        {
            var systemProjectionsTypes = handlerTypeContainer.Items.Where(x => x is ISystemProjection);

            try
            {
                foreach (var systemProjectionType in systemProjectionsTypes)
                {
                    //projectionReader.Get(new StupidId(""), systemProjectionType);
                }
            }
            catch (Exception)
            {
                return false;
            }


            return true;
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
