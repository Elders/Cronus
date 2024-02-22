using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;

namespace Elders.Cronus.EventStore.Index.Handlers;

[DataContract(Name = "055f2407-6b5a-4f77-92b0-fcae4c8d86a7")]
public class EventStoreIndexBuilder : Saga, ISystemSaga,
    IEventHandler<EventStoreIndexRequested>,
    ISagaTimeoutHandler<RebuildIndexInternal>,
    ISagaTimeoutHandler<EventStoreIndexRebuildTimedout>
{
    private readonly ICronusJobRunner jobRunner;
    private readonly IRebuildIndex_EventToAggregateRootId_JobFactory jobFactory;
    private readonly RebuildIndex_MessageCounter_JobFactory messageCounterJobFactory;

    public EventStoreIndexBuilder(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, ICronusJobRunner jobRunner, IRebuildIndex_EventToAggregateRootId_JobFactory jobFactory, RebuildIndex_MessageCounter_JobFactory messageCounterJobFactory)
        : base(commandPublisher, timeoutRequestPublisher)
    {
        this.jobRunner = jobRunner;
        this.jobFactory = jobFactory;
        this.messageCounterJobFactory = messageCounterJobFactory;
    }

    public Task HandleAsync(EventStoreIndexRequested @event)
    {
        var startRebuildAt = @event.Timebox.RequestStartAt;
        if (startRebuildAt.AddMinutes(5) > DateTime.UtcNow && @event.Timebox.HasExpired == false)
        {
            RequestTimeout(new RebuildIndexInternal(@event, @event.Timebox.RequestStartAt, @event.MaxDegreeOfParallelism));
            //RequestTimeout(new EventStoreIndexRebuildTimedout(@event, @event.Timebox.FinishRequestUntil));
        }

        return Task.CompletedTask;
    }

    public async Task HandleAsync(RebuildIndexInternal sagaTimeout)
    {
        ICronusJob<object> job = null;
        // we need to redesign the job factories
        var theId = sagaTimeout.EventStoreIndexRequest.Id.Id;

        if (theId.Equals(typeof(MessageCounterIndex).GetContractId(), StringComparison.OrdinalIgnoreCase))
        {
            job = messageCounterJobFactory.CreateJob(sagaTimeout.EventStoreIndexRequest.Timebox);
        }
        else
        {
            job = jobFactory.CreateJob(sagaTimeout.EventStoreIndexRequest.Timebox, sagaTimeout.MaxDegreeOfParallelism);
        }

        JobExecutionStatus result = await jobRunner.ExecuteAsync(job).ConfigureAwait(false);

        if (result == JobExecutionStatus.Running)
        {
            RequestTimeout(new RebuildIndexInternal(sagaTimeout.EventStoreIndexRequest, DateTime.UtcNow.AddSeconds(30), sagaTimeout.MaxDegreeOfParallelism));
        }
        else if (result == JobExecutionStatus.Failed)
        {
            // log error
            RequestTimeout(new RebuildIndexInternal(sagaTimeout.EventStoreIndexRequest, DateTime.UtcNow.AddSeconds(30), sagaTimeout.MaxDegreeOfParallelism));
        }
        else if (result == JobExecutionStatus.Completed)
        {
            var finalize = new FinalizeEventStoreIndexRequest(sagaTimeout.EventStoreIndexRequest.Id);
            commandPublisher.Publish(finalize);
        }
    }

    public Task HandleAsync(EventStoreIndexRebuildTimedout sagaTimeout)
    {
        //var timedout = new TimeoutProjectionVersionRequest(sagaTimeout.ProjectionVersionRequest.Id, sagaTimeout.ProjectionVersionRequest.Version, sagaTimeout.ProjectionVersionRequest.Timebox);
        //commandPublisher.Publish(timedout);
        return Task.CompletedTask;
    }
}

[DataContract(Name = "09d3f870-66f5-4f00-aedd-659b719791fe")]
public sealed class RebuildIndexInternal : ISystemScheduledMessage
{
    RebuildIndexInternal()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    public RebuildIndexInternal(EventStoreIndexRequested indexRequest, DateTime publishAt, int maxDegreeOfParallelism) : this()
    {
        EventStoreIndexRequest = indexRequest;
        PublishAt = publishAt;
        MaxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    [DataMember(Order = 1)]
    public EventStoreIndexRequested EventStoreIndexRequest { get; private set; }

    [DataMember(Order = 2)]
    public DateTime PublishAt { get; set; }

    [DataMember(Order = 3)]
    public DateTimeOffset Timestamp { get; private set; }

    [DataMember(Order = 4)]
    public int MaxDegreeOfParallelism { get; private set; }

    public string Tenant { get { return EventStoreIndexRequest.Id.Tenant; } }
}

[DataContract(Name = "4f6c585f-31c7-4bcb-867c-2c38071c29f3")]
public sealed class EventStoreIndexRebuildTimedout : ISystemScheduledMessage
{
    EventStoreIndexRebuildTimedout()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    public EventStoreIndexRebuildTimedout(EventStoreIndexRequested eventStoreIndexRequest, DateTime publishAt) : this()
    {
        EventStoreIndexRequest = eventStoreIndexRequest;
        PublishAt = publishAt;
    }

    [DataMember(Order = 1)]
    public EventStoreIndexRequested EventStoreIndexRequest { get; private set; }

    [DataMember(Order = 2)]
    public DateTime PublishAt { get; set; }

    [DataMember(Order = 2)]
    public DateTimeOffset Timestamp { get; private set; }

    public string Tenant { get { return EventStoreIndexRequest.Id.Tenant; } }
}
