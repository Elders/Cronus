using System;
using System.Runtime.Serialization;
using System.Threading;
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

    /// <summary>
    /// Handles the <see cref="EventStoreIndexRequested"/> event by scheduling a saga timeout that drives the rebuild loop.
    /// </summary>
    /// <param name="event">The event signalling that an index rebuild was requested.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(EventStoreIndexRequested @event, CancellationToken cancellationToken = default)
    {
        var startRebuildAt = @event.Timebox.RequestStartAt;
        if (startRebuildAt.AddMinutes(5) > DateTime.UtcNow && @event.Timebox.HasExpired == false)
        {
            await RequestTimeoutAsync(new RebuildIndexInternal(@event, @event.Timebox.RequestStartAt, @event.MaxDegreeOfParallelism), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles a periodic <see cref="RebuildIndexInternal"/> saga timeout by running the next slice of the rebuild job and re-arming or finalising the saga.
    /// </summary>
    /// <param name="sagaTimeout">The saga timeout that fired.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(RebuildIndexInternal sagaTimeout, CancellationToken cancellationToken = default)
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

        JobExecutionStatus result = await jobRunner.ExecuteAsync(job, cancellationToken).ConfigureAwait(false);

        if (result == JobExecutionStatus.Running)
        {
            await RequestTimeoutAsync(new RebuildIndexInternal(sagaTimeout.EventStoreIndexRequest, DateTime.UtcNow.AddSeconds(60), sagaTimeout.MaxDegreeOfParallelism), cancellationToken).ConfigureAwait(false);
        }
        else if (result == JobExecutionStatus.Failed)
        {
            // log error
            await RequestTimeoutAsync(new RebuildIndexInternal(sagaTimeout.EventStoreIndexRequest, DateTime.UtcNow.AddSeconds(60), sagaTimeout.MaxDegreeOfParallelism), cancellationToken).ConfigureAwait(false);
        }
        else if (result == JobExecutionStatus.Completed)
        {
            var finalize = new FinalizeEventStoreIndexRequest(sagaTimeout.EventStoreIndexRequest.Id);
            await commandPublisher.PublishAsync(finalize, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles the <see cref="EventStoreIndexRebuildTimedout"/> saga timeout (currently a no-op).
    /// </summary>
    /// <param name="sagaTimeout">The saga timeout that fired.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(EventStoreIndexRebuildTimedout sagaTimeout, CancellationToken cancellationToken = default)
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
