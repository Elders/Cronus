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
                        var state = aggregateRootResult.Data.State;
                        var revision = aggregateRootResult.Data.Revision;
                        RequestSnapshot(arType, state, revision, result.Data.EventsCount, sw.Elapsed);
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
                        RequestSnapshot(arType, aggregateRoot.State, aggregateRoot.Revision, eventStream.EventsCount, sw.Elapsed);
                        return new ReadResult<AR>(aggregateRoot);
                    }

                    return ReadResult<AR>.WithNotFoundHint($"Unable to load aggregate root with id {id.Value} from snapshot with revision {snapshot.Revision}.");
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

        private void RequestSnapshot(Type arType, IAggregateRootState aggregateRootState, int revision, int eventsLoaded, TimeSpan loadTime)
        {
            logger.Debug(() => "Aggregate root {type} with revision {revision} was loaded for {time} from {eventsCount} events. Requesting snapshot...",
                arType.Name, revision, loadTime, eventsLoaded);

            var published = publisher.Publish(new RequestSnapshot(new SnapshotManagerId(aggregateRootState.Id, aggregateRootState.Id.Tenant), revision, arType.GetContractId(), eventsLoaded, loadTime));
            if (published == false)
                logger.Warn(() => "Failed to publish {cmd} command.", nameof(Snapshots.RequestSnapshot));
        }
    }
}
