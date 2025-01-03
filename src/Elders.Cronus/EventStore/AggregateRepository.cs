using System;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Userfull;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore;

public sealed class AggregateRepository : IAggregateRepository
{
    readonly IAggregateRootAtomicAction atomicAction;
    readonly IEventStore eventStore;
    readonly IIntegrityPolicy<EventStream> integrityPolicy;
    private readonly CronusAggregateCommitInterceptor aggregateInterceptor;
    private readonly ILogger<AggregateRepository> logger;

    public AggregateRepository(IEventStoreFactory eventStoreFactory, IAggregateRootAtomicAction atomicAction, IIntegrityPolicy<EventStream> integrityPolicy, CronusAggregateCommitInterceptor aggregateInterceptor, ILogger<AggregateRepository> logger)
    {
        if (eventStoreFactory is null) throw new ArgumentNullException(nameof(eventStoreFactory));
        if (atomicAction is null) throw new ArgumentNullException(nameof(atomicAction));
        if (integrityPolicy is null) throw new ArgumentNullException(nameof(integrityPolicy));

        this.eventStore = eventStoreFactory.GetEventStore();
        this.atomicAction = atomicAction;
        this.integrityPolicy = integrityPolicy;
        this.aggregateInterceptor = aggregateInterceptor;
        this.logger = logger;
    }

    /// <summary>
    /// Saves the specified aggregate root.
    /// </summary>
    /// <typeparam name="AR">The type of the aggregate root.</typeparam>
    /// <param name="aggregateRoot">The aggregate root.</param>
    /// <exception cref="Elders.Cronus.DomainModeling.AggregateRootException"></exception>
    public Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
    {
        return SaveInternalAsync(aggregateRoot);
    }

    /// <summary>
    /// Loads an aggregate with the specified identifier.
    /// </summary>
    /// <typeparam name="AR">The type of the r.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public async Task<ReadResult<AR>> LoadAsync<AR>(AggregateRootId id) where AR : IAggregateRoot
    {
        EventStream eventStream = await eventStore.LoadAsync(id).ConfigureAwait(false);
        var integrityResult = integrityPolicy.Apply(eventStream);
        if (integrityResult.IsIntegrityViolated)
            throw new EventStreamIntegrityViolationException($"AR integrity is violated for ID={id.Value}");
        eventStream = integrityResult.Output;
        AR aggregateRoot;
        if (eventStream.TryRestoreFromHistory(out aggregateRoot) == false) // this should be a sync operation, it's just triggers internal in-memory handlers for an aggregate
            return ReadResult<AR>.WithNotFoundHint($"Unable to load AR with ID={id.Value}");

        return new ReadResult<AR>(aggregateRoot);
    }

    internal async Task<AggregateCommit> SaveInternalAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
    {
        if (aggregateRoot.UncommittedEvents is null || aggregateRoot.UncommittedEvents.Any() == false)
        {
            if (aggregateRoot.UncommittedPublicEvents is not null && aggregateRoot.UncommittedPublicEvents.Any())
            {
                throw new Exception("Public events cannot be applied by themselves. If you want to publish a public event you need to have at least one private event (IEvent) within the same commit. It is not recommended to update aggregate state with public events!");
            }

            return default;
        }

        DateTimeOffset lastTimestamp = default;
        foreach (var @event in aggregateRoot.UncommittedEvents)
        {
            if (@event.Timestamp == default)
                throw new InvalidOperationException("Cannot use default timestamp for an aggregate event.");

            if (lastTimestamp == @event.Timestamp)
            {
                var errorMessage = "There are multiple events with the same timestamp within the same aggregate commit. Loss of data may occur!";
#if DEBUG
                throw new InvalidOperationException(errorMessage);
#else
                logger?.LogWarning(errorMessage);
#endif
            }

            lastTimestamp = @event.Timestamp;
        }

        var arCommit = new AggregateCommit(aggregateRoot.State.Id.RawId, aggregateRoot.Revision, aggregateRoot.UncommittedEvents.ToList(), aggregateRoot.UncommittedPublicEvents.ToList(), DateTime.UtcNow.ToFileTimeUtc());
        arCommit = await aggregateInterceptor.OnAppendingAsync(arCommit).ConfigureAwait(false);

        var result = await atomicAction.ExecuteAsync(aggregateRoot.State.Id, aggregateRoot.Revision, async () => await eventStore.AppendAsync(arCommit).ConfigureAwait(false)).ConfigureAwait(false);

        if (result.IsSuccessful)
        {
            // #prodalzavameNapred
            // #bravoKobra
            // https://www.youtube.com/watch?v=2wWusHu_3w8

            await aggregateInterceptor.OnAppendAsync(arCommit).ConfigureAwait(false);

            return arCommit;
        }
        else
        {
            throw new AggregateStateFirstLevelConcurrencyException($"Unable to save AR {Environment.NewLine}{arCommit.ToString()}", result.Errors.MakeJustOneException());
        }
    }
}
