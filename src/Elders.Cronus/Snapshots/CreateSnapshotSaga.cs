using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;

namespace Elders.Cronus.Snapshots
{
    public sealed class CreateSnapshotSaga : Saga, ISystemSaga,
        IEventHandler<SnapshotRequested>,
        ISagaTimeoutHandler<CreateSnapshotScheduledMessage>
    {
        private readonly ICronusJobRunner jobRunner;
        private readonly CreateSnapshot_JobFactory createSnapshot_JobFactory;

        public CreateSnapshotSaga(ICronusJobRunner jobRunner, IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher, CreateSnapshot_JobFactory createSnapshot_JobFactory)
            : base(commandPublisher, timeoutRequestPublisher)
        {
            this.jobRunner = jobRunner;
            this.createSnapshot_JobFactory = createSnapshot_JobFactory;
        }

        public Task HandleAsync(SnapshotRequested @event)
        {
            RequestTimeout(new CreateSnapshotScheduledMessage(@event, @event.Timestamp.DateTime));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(CreateSnapshotScheduledMessage sagaTimeout)
        {
            var job = createSnapshot_JobFactory.CreateJob(
                sagaTimeout.SnapshotRequested.Id.AggregateId,
                sagaTimeout.SnapshotRequested.AggregateContract,
                sagaTimeout.SnapshotRequested.Revision,
                sagaTimeout.SnapshotRequested.Timestamp);

            JobExecutionStatus result = await jobRunner.ExecuteAsync(job).ConfigureAwait(false);

            if (result == JobExecutionStatus.Running)
            {
                RequestTimeout(new CreateSnapshotScheduledMessage(sagaTimeout.SnapshotRequested, DateTime.UtcNow.AddSeconds(30)));
            }
            else if (result == JobExecutionStatus.Failed)
            {
                var failed = new MarkSnapshotCreationAsFailed(sagaTimeout.SnapshotRequested.Id, sagaTimeout.SnapshotRequested.Revision);
                commandPublisher.Publish(failed);
            }
            else if (result == JobExecutionStatus.Completed)
            {
                var created = new MarkSnapshotAsCreated(sagaTimeout.SnapshotRequested.Id, sagaTimeout.SnapshotRequested.Revision);
                commandPublisher.Publish(created);
            }
            else if (result == JobExecutionStatus.Canceled)
            {
                var canceled = new MarkSnapshotAsCanceled(sagaTimeout.SnapshotRequested.Id, sagaTimeout.SnapshotRequested.Revision);
                commandPublisher.Publish(canceled);
            }
        }
    }

    [DataContract(Name = "7fc355fb-9e33-4bf7-8620-9fe645eba8f4")]
    public class CreateSnapshotScheduledMessage : ISystemScheduledMessage
    {
        CreateSnapshotScheduledMessage() { }

        public CreateSnapshotScheduledMessage(SnapshotRequested snapshotRequested, DateTime publishAt)
        {
            SnapshotRequested = snapshotRequested;
            PublishAt = publishAt;
        }

        [DataMember(Order = 1)]
        public SnapshotRequested SnapshotRequested { get; private set; }

        [DataMember(Order = 2)]
        public DateTime PublishAt { get; set; }

        public string Tenant { get { return SnapshotRequested.Id.Tenant; } }
    }

    [DataContract(Name = "b8af24bb-598c-4683-9416-08c120514b62")]
    public class MarkSnapshotCreationAsFailed : ISystemCommand
    {
        MarkSnapshotCreationAsFailed() { }

        public MarkSnapshotCreationAsFailed(SnapshotManagerId id, int revision)
        {
            Id = id;
            Revision = revision;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }
    }

    [DataContract(Name = "c29ed844-58da-4196-81f3-96b29ad1628f")]
    public class MarkSnapshotAsCreated : ISystemCommand
    {
        MarkSnapshotAsCreated() { }

        public MarkSnapshotAsCreated(SnapshotManagerId id, int revision)
        {
            Id = id;
            Revision = revision;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }
    }

    [DataContract(Name = "9047662e-2be2-4df6-98c6-0efc79baf0a4")]
    public class MarkSnapshotAsCanceled : ISystemCommand
    {
        MarkSnapshotAsCanceled() { }

        public MarkSnapshotAsCanceled(SnapshotManagerId id, int revision)
        {
            Id = id;
            Revision = revision;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }
    }
}
