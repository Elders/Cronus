using System.Collections.Generic;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.InMemory;
using Elders.Cronus.EventStore.Integrity;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite;

[Subject("AggregateRoot")]
public class When_loading_aggregate_root_from_event_store
{
    Establish context = async () =>
    {

        versionService = new InMemoryAggregateRootAtomicAction();
        eventStoreStorage = new InMemoryEventStoreStorage();
        eventStore = new InMemoryEventStore(eventStoreStorage);
        eventStoreFactory = new EventStoreFactory(eventStore, null);
        eventStorePlayer = new InMemoryEventStorePlayer(eventStoreStorage);
        integrityPpolicy = new EventStreamIntegrityPolicy(null);
        aggregateRepository = new AggregateRepository(eventStoreFactory, versionService, integrityPpolicy, new CronusAggregateCommitInterceptor(new List<EmptyAggregateTransformer>()), null);
        id = new TestAggregateId();
        aggregateRoot = new TestAggregateRoot(id);
        await aggregateRepository.SaveAsync<TestAggregateRoot>(aggregateRoot);
        var events = await aggregateRepository.LoadAsync<TestAggregateRoot>(id);
        aggregateRoot = events.Data;
        aggregateRoot.DoSomething("When_build_aggregate_root_from_events");
        await aggregateRepository.SaveAsync<TestAggregateRoot>(aggregateRoot);
    };

    Because of = () => loadedAggregateRoot = aggregateRepository.LoadAsync<TestAggregateRoot>(id).GetAwaiter().GetResult().Data;

    It should_instansiate_aggregate_root = () => loadedAggregateRoot.ShouldNotBeNull();

    It should_instansiate_aggregate_root_with_valid_state = () => loadedAggregateRoot.State.Id.ShouldEqual(id);

    It should_instansiate_aggregate_root_with_latest_state = () => loadedAggregateRoot.State.UpdatableField.ShouldEqual("When_build_aggregate_root_from_events");

    It should_instansiate_aggregate_root_with_latest_state_version = () => (loadedAggregateRoot as IAggregateRoot).Revision.ShouldEqual(2);

    static TestAggregateId id;
    static InMemoryEventStoreStorage eventStoreStorage;
    static IAggregateRootAtomicAction versionService;
    static IEventStore eventStore;
    static IEventStorePlayer eventStorePlayer;
    static IAggregateRepository aggregateRepository;
    static TestAggregateRoot aggregateRoot;
    static TestAggregateRoot loadedAggregateRoot;
    static IIntegrityPolicy<EventStream> integrityPpolicy;
    static EventStoreFactory eventStoreFactory;
}
public class InMemoryAggregateRootAtomicAction : IAggregateRootAtomicAction
{
    readonly MemoryCache aggregateLock = null;
    readonly MemoryCache aggregateRevisions = null;
    readonly MemoryCacheEntryOptions cacheEntryOptions;

    public InMemoryAggregateRootAtomicAction()
    {
        var aggregateLockMemoryCacheOptions = new MemoryCacheOptions()
        {
            SizeLimit = 500 * 1024 * 1024, //500mb
            CompactionPercentage = 0.1, // 10%
            ExpirationScanFrequency = new TimeSpan(0, 1, 0)
        };

        var aggregateRevisionsMemoryCacheOptions = new MemoryCacheOptions()
        {
            SizeLimit = 500 * 1024 * 1024, //500mb
            CompactionPercentage = 0.1, // 10%
            ExpirationScanFrequency = new TimeSpan(0, 1, 0)
        };

        cacheEntryOptions = new MemoryCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromSeconds(30d),
            Size = 1
        };

        aggregateLock = new MemoryCache(aggregateLockMemoryCacheOptions);
        aggregateRevisions = new MemoryCache(aggregateRevisionsMemoryCacheOptions);
    }

    public async Task<Elders.Cronus.Userfull.Result<bool>> ExecuteAsync(AggregateRootId aggregateRootId, int aggregateRootRevision, Func<Task> action)
    {
        var result = new Elders.Cronus.Userfull.Result<bool>(false);
        var acquired = new AtomicBoolean(false);

        try
        {
            acquired = aggregateLock.Get(aggregateRootId.Value) as AtomicBoolean;
            if (acquired is null)
            {
                acquired = aggregateLock.Set(aggregateRootId.Value, new AtomicBoolean(false), cacheEntryOptions);
                if (acquired is null)
                    return result;
            }

            if (acquired.CompareAndSet(false, true))
            {
                try
                {
                    AtomicInteger revision = aggregateRevisions.Get(aggregateRootId.Value) as AtomicInteger;
                    if (revision is null)
                    {
                        var newRevision = new AtomicInteger(aggregateRootRevision - 1);
                        revision = aggregateRevisions.Set(aggregateRootId.Value, newRevision, cacheEntryOptions);
                        if (revision is null)
                            return result;
                    }

                    var currentRevision = revision.Value;
                    if (revision.CompareAndSet(aggregateRootRevision - 1, aggregateRootRevision))
                    {
                        try
                        {
                            await action().ConfigureAwait(false);
                            return Elders.Cronus.Userfull.Result.Success;
                        }
                        catch (Exception)
                        {
                            revision.GetAndSet(currentRevision);
                            throw;
                        }
                    }
                }
                finally
                {
                    acquired.GetAndSet(false);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            return Elders.Cronus.Userfull.Result.Error(ex);
        }
    }

    public void Dispose()
    {
        aggregateRevisions?.Dispose();
        aggregateLock?.Dispose();
    }
}

[Serializable]
public class AtomicBoolean : IFormattable
{
    private volatile int booleanValue;

    // <summary>
    // Gets or sets the current value.
    // </summary>
    public bool Value
    {
        get { return this.booleanValue != 0; }
        set { this.booleanValue = value ? 1 : 0; }
    }

    public AtomicBoolean()
        : this(false)
    {
    }
    public AtomicBoolean(bool initialValue)
    {
        Value = initialValue;
    }

    public bool CompareAndSet(bool expect, bool update)
    {
        int expectedIntValue = expect ? 1 : 0;
        int newIntValue = update ? 1 : 0;
        return Interlocked.CompareExchange(ref this.booleanValue, newIntValue, expectedIntValue) == expectedIntValue;
    }
    public bool GetAndSet(bool newValue)
    {
        return Interlocked.Exchange(ref this.booleanValue, newValue ? 1 : 0) != 0;
    }
    public bool WeakCompareAndSet(bool expect, bool update)
    {
        return CompareAndSet(expect, update);
    }

    public override bool Equals(object obj)
    {
        return obj as AtomicBoolean == this;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return ToString(CultureInfo.CurrentCulture);
    }

    public string ToString(IFormatProvider formatProvider)
    {
        return Value.ToString(formatProvider);
    }

    public string ToString(string format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return Value.ToString(formatProvider);
    }

    public static bool operator ==(AtomicBoolean left, AtomicBoolean right)
    {
        if (left is null || right is null)
            return false;

        return left.Value == right.Value;
    }

    public static bool operator !=(AtomicBoolean left, AtomicBoolean right)
    {
        return !(left == right);
    }

    public static implicit operator bool(AtomicBoolean atomic)
    {
        if (atomic == null) { return false; }
        else { return atomic.Value; }
    }
}

[Serializable]
public class AtomicInteger : IFormattable
{
    private volatile int integerValue;

    // <summary>
    // Gets or sets the current value.
    // </summary>
    public int Value
    {
        get { return this.integerValue; }
        set { this.integerValue = value; }
    }

    public AtomicInteger()
        : this(0)
    {
    }
    public AtomicInteger(int initialValue)
    {
        this.integerValue = initialValue;
    }

    public int AddAndGet(int delta)
    {
        return Interlocked.Add(ref this.integerValue, delta);
    }
    public bool CompareAndSet(int expect, int update)
    {
        return Interlocked.CompareExchange(ref this.integerValue, update, expect) == expect;
    }
    public int DecrementAndGet()
    {
        return Interlocked.Decrement(ref this.integerValue);
    }
    public int GetAndDecrement()
    {
        return Interlocked.Decrement(ref this.integerValue) + 1;
    }
    public int GetAndIncrement()
    {
        return Interlocked.Increment(ref this.integerValue) - 1;
    }
    public int GetAndSet(int newValue)
    {
        return Interlocked.Exchange(ref this.integerValue, newValue);
    }
    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref this.integerValue);
    }
    public bool WeakCompareAndSet(int expect, int update)
    {
        return CompareAndSet(expect, update);
    }

    public override bool Equals(object obj)
    {
        return obj as AtomicInteger == this;
    }
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    public override string ToString()
    {
        return ToString(CultureInfo.CurrentCulture);
    }
    public string ToString(IFormatProvider formatProvider)
    {
        return Value.ToString(formatProvider);
    }
    public string ToString(string format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }
    public string ToString(string format, IFormatProvider formatProvider)
    {
        return Value.ToString(formatProvider);
    }

    public static bool operator ==(AtomicInteger left, AtomicInteger right)
    {
        if (left is null || right is null)
            return false;

        return left.Value == right.Value;
    }
    public static bool operator !=(AtomicInteger left, AtomicInteger right)
    {
        return !(left == right);
    }
    public static implicit operator int(AtomicInteger atomic)
    {
        if (atomic == null)
        {
            return 0;
        }
        else
        {
            return atomic.Value;
        }
    }
}
