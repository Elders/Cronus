using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore.Index.Events;
using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index.Handlers
{
    public class EventStoreIndexBuilder : Saga,
        IEventHandler<EventStoreIndexRequested>,
        ISagaTimeoutHandler<RebuildIndexInternal>,
        ISagaTimeoutHandler<EventStoreIndexRebuildTimedout>
    {
        private readonly ICronusJobRunner jobRunner;

        public EventStoreIndexBuilder(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, ICronusJobRunner jobRunner)
            : base(commandPublisher, timeoutRequestPublisher)
        {
            this.jobRunner = jobRunner;
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
            var result = jobRunner.ExecuteAsync(null).GetAwaiter().GetResult();

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
                // finish
            }
        }

        public void Handle(EventStoreIndexRebuildTimedout sagaTimeout)
        {
            //var timedout = new TimeoutProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
            //commandPublisher.Publish(timedout);
        }
    }

    [DataContract(Name = "09d3f870-66f5-4f00-aedd-659b719791fe")]
    public class RebuildIndexInternal : IScheduledMessage
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
    public class EventStoreIndexRebuildTimedout : IScheduledMessage
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
