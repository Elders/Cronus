using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Snapshots;
using Elders.Cronus.Snapshots.SnapshotStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore
{
    public sealed class SnapshottingAggregateRepository : IAggregateRepository
    {
        private readonly AggregateRepository realDeal;
        private readonly IEventStore eventStore;
        private readonly ISnapshotReader snapshotReader;
        private readonly IIntegrityPolicy<EventStream> integrityPolicy;
        private readonly IPublisher<ISystemCommand> publisher;
        private readonly ILogger<SnapshottingAggregateRepository> logger;

        public SnapshottingAggregateRepository(AggregateRepository realDeal, EventStoreFactory eventStoreFactory, ISnapshotReader snapshotReader, IIntegrityPolicy<EventStream> integrityPolicy, IPublisher<ISystemCommand> publisher, ILogger<SnapshottingAggregateRepository> logger)
        {
            this.realDeal = realDeal;
            eventStore = eventStoreFactory.GetEventStore();
            this.snapshotReader = snapshotReader;
            this.integrityPolicy = integrityPolicy;
            this.publisher = publisher;
            this.logger = logger;
        }

        public async Task<ReadResult<AR>> LoadAsync<AR>(AggregateRootId id) where AR : IAggregateRoot
        {
            var arType = typeof(AR);
            if (arType.IsSnapshotable())
            {
                Snapshot snapshot = await snapshotReader.ReadAsync(id);
                if (snapshot is null)
                {
                    var sw = Stopwatch.StartNew();
                    var result = await realDeal.LoadInternalAsync<AR>(id).ConfigureAwait(false);
                    sw.Stop();
                    if (result.IsSuccess)
                    {
                        var aggregateRootResult = result.Data.AggregateRootReadResult;
                        RequestSnapshot(aggregateRootResult.Data, result.Data.EventsCount, sw.Elapsed);
                        return aggregateRootResult;
                    }

                    return ReadResult<AR>.WithNotFoundHint(result.NotFoundHint);
                }
                else
                {
                    var sw = Stopwatch.StartNew();
                    EventStream eventStream = await eventStore.LoadAsync(id, snapshot.Revision).ConfigureAwait(false);
                    var integrityResult = integrityPolicy.Apply(eventStream);
                    if (integrityResult.IsIntegrityViolated)
                        throw new EventStreamIntegrityViolationException($"Aggregare root integrity is violated for id {id.Value} and snapshot revision {snapshot.Revision}");
                    eventStream = integrityResult.Output;

                    if (eventStream.TryRestoreFromSnapshot(snapshot.State, snapshot.Revision, out AR aggregateRoot))
                    {
                        sw.Stop();
                        RequestSnapshot(aggregateRoot, eventStream.EventsCount, sw.Elapsed);
                        return new ReadResult<AR>(aggregateRoot);
                    }

                    var result = await realDeal.LoadAsync<AR>(id).ConfigureAwait(false);
                    return result;
                }
            }
            else
            {
                var result = await realDeal.LoadAsync<AR>(id).ConfigureAwait(false);
                return result;
            }
        }

        public Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
            => realDeal.SaveAsync(aggregateRoot);

        internal Task<AggregateCommit> SaveInternalAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
            => realDeal.SaveInternalAsync(aggregateRoot);

        private void RequestSnapshot(IAggregateRoot aggregateRoot, int eventsLoaded, TimeSpan loadTime)
        {
            var arType = aggregateRoot.GetType();
            logger.Debug(() => "Aggregate root {type} with revision {revision} was loaded for {time} from {eventsCount} events. Requesting snapshot...",
                arType.Name, aggregateRoot.Revision, loadTime, eventsLoaded);

            var id = new SnapshotManagerId(aggregateRoot.State.Id, aggregateRoot.State.Id.Tenant);
            var published = publisher.Publish(
                new RequestSnapshot(
                    id,
                    aggregateRoot.Revision,
                    arType.GetContractId(),
                    eventsLoaded,
                    loadTime));

            if (published == false)
                logger.Warn(() => "Failed to publish {cmd} command.", nameof(Snapshots.RequestSnapshot));
        }
    }
}
