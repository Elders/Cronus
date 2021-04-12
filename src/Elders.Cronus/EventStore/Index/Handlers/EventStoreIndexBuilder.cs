using Elders.Cronus.Cluster.Job;
using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index.Handlers
{
    [DataContract(Name = "055f2407-6b5a-4f77-92b0-fcae4c8d86a7")]
    public class EventStoreIndexBuilder : Saga, ISystemSaga,
        IEventHandler<EventStoreIndexRequested>,
        ISagaTimeoutHandler<RebuildIndexInternal>,
        ISagaTimeoutHandler<EventStoreIndexRebuildTimedout>
    {
        private readonly ICronusJobRunner jobRunner;
        private readonly RebuildIndex_EventToAggregateRootId_JobFactory jobFactory;
        private readonly RebuildIndex_MessageCounter_JobFactory messageCounterJobFactory;

        public EventStoreIndexBuilder(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, ICronusJobRunner jobRunner, RebuildIndex_EventToAggregateRootId_JobFactory jobFactory, RebuildIndex_MessageCounter_JobFactory messageCounterJobFactory)
            : base(commandPublisher, timeoutRequestPublisher)
        {
            this.jobRunner = jobRunner;
            this.jobFactory = jobFactory;
            this.messageCounterJobFactory = messageCounterJobFactory;
        }

        public void Handle(EventStoreIndexRequested @event)
        {
            var startRebuildAt = @event.Timebox.RebuildStartAt;
            if (startRebuildAt.AddMinutes(5) > DateTime.UtcNow && @event.Timebox.HasExpired == false)
            {
                RequestTimeout(new RebuildIndexInternal(@event, @event.Timebox.RebuildStartAt));
                RequestTimeout(new EventStoreIndexRebuildTimedout(@event, @event.Timebox.RebuildFinishUntil));
            }
        }

        public void Handle(RebuildIndexInternal sagaTimeout)
        {
            ICronusJob<object> job = null;
            // we need to redesign the job factories
            var theId = sagaTimeout.EventStoreIndexRequest.Id.Id;
            if (theId.Equals(typeof(EventToAggregateRootId).GetContractId(), StringComparison.OrdinalIgnoreCase))
            {
                job = jobFactory.CreateJob(sagaTimeout.EventStoreIndexRequest.Timebox);
            }
            else if (theId.Equals(typeof(MessageCounterIndex).GetContractId(), StringComparison.OrdinalIgnoreCase))
            {
                job = messageCounterJobFactory.CreateJob(sagaTimeout.EventStoreIndexRequest.Timebox);
            }
            else
            {
                return;
            }

            var result = jobRunner.ExecuteAsync(job).GetAwaiter().GetResult();

            if (result == JobExecutionStatus.Running)
            {
                RequestTimeout(new RebuildIndexInternal(sagaTimeout.EventStoreIndexRequest, DateTime.UtcNow.AddSeconds(30)));
            }
            else if (result == JobExecutionStatus.Failed)
            {
                // log error
                RequestTimeout(new RebuildIndexInternal(sagaTimeout.EventStoreIndexRequest, DateTime.UtcNow.AddSeconds(30)));
            }
            else if (result == JobExecutionStatus.Completed)
            {
                var finalize = new FinalizeEventStoreIndexRequest(sagaTimeout.EventStoreIndexRequest.Id);
                commandPublisher.Publish(finalize);
            }
        }

        public void Handle(EventStoreIndexRebuildTimedout sagaTimeout)
        {
            //var timedout = new TimeoutProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
            //commandPublisher.Publish(timedout);
        }
    }

    [DataContract(Name = "09d3f870-66f5-4f00-aedd-659b719791fe")]
    public class RebuildIndexInternal : ISystemScheduledMessage
    {
        RebuildIndexInternal() { }

        public RebuildIndexInternal(EventStoreIndexRequested indexRequest, DateTime publishAt)
        {
            EventStoreIndexRequest = indexRequest;
            PublishAt = publishAt;
        }

        [DataMember(Order = 1)]
        public EventStoreIndexRequested EventStoreIndexRequest { get; private set; }

        [DataMember(Order = 2)]
        public DateTime PublishAt { get; set; }

        public string Tenant { get { return EventStoreIndexRequest.Id.Tenant; } }
    }

    [DataContract(Name = "4f6c585f-31c7-4bcb-867c-2c38071c29f3")]
    public class EventStoreIndexRebuildTimedout : ISystemScheduledMessage
    {
        EventStoreIndexRebuildTimedout() { }

        public EventStoreIndexRebuildTimedout(EventStoreIndexRequested eventStoreIndexRequest, DateTime publishAt)
        {
            EventStoreIndexRequest = eventStoreIndexRequest;
            PublishAt = publishAt;
        }

        [DataMember(Order = 1)]
        public EventStoreIndexRequested EventStoreIndexRequest { get; private set; }

        [DataMember(Order = 2)]
        public DateTime PublishAt { get; set; }

        public string Tenant { get { return EventStoreIndexRequest.Id.Tenant; } }
    }
}
