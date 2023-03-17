using System;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Snapshots.Job;

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
                sagaTimeout.SnapshotRequested.Id.InstanceId,
                sagaTimeout.SnapshotRequested.Contract,
                sagaTimeout.SnapshotRequested.NewRevisionStatus.Revision,
                sagaTimeout.SnapshotRequested.Timestamp);

            JobExecutionStatus result = await jobRunner.ExecuteAsync(job).ConfigureAwait(false);

            if (result == JobExecutionStatus.Running)
            {
                RequestTimeout(new CreateSnapshotScheduledMessage(sagaTimeout.SnapshotRequested, DateTime.UtcNow.AddSeconds(30)));
            }
            else if (result == JobExecutionStatus.Failed)
            {
                var failed = new FailSnapshotCreation(sagaTimeout.SnapshotRequested.Id, sagaTimeout.SnapshotRequested.NewRevisionStatus.Revision);
                commandPublisher.Publish(failed);
            }
            else if (result == JobExecutionStatus.Completed)
            {
                var created = new CompleteSnapshot(sagaTimeout.SnapshotRequested.Id, sagaTimeout.SnapshotRequested.NewRevisionStatus.Revision);
                commandPublisher.Publish(created);
            }
            else if (result == JobExecutionStatus.Canceled)
            {
                var canceled = new CancelSnapshot(sagaTimeout.SnapshotRequested.Id, sagaTimeout.SnapshotRequested.NewRevisionStatus.Revision);
                commandPublisher.Publish(canceled);
            }
        }
    }
}
