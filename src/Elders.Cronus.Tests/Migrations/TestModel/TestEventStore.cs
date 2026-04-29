using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel;

public class TestEventStore : IEventStore
{
    public IList<AggregateCommit> Storage { get; private set; }

    public TestEventStore()
    {
        Storage = new List<AggregateCommit>();
    }

    public Task AppendAsync(AggregateCommit aggregateCommit, CancellationToken cancellationToken = default)
    {
        Storage.Add(aggregateCommit);

        return Task.CompletedTask;
    }


    public Task AppendAsync(AggregateEventRaw aggregateCommitRaw, CancellationToken cancellationToken = default)
    {
        return Task.FromException(new System.NotImplementedException());
    }
    public Task<EventStream> LoadAsync(IBlobId aggregateId, CancellationToken cancellationToken = default)
    {
        var es = new EventStream(Storage.Where(x => x.AggregateRootId.Span.SequenceEqual(aggregateId.RawId.Span)).ToList());
        return Task.FromResult(es);
    }

    public Task<bool> DeleteAsync(AggregateEventRaw eventRaw, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
    public Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingAsync(IBlobId aggregateId, PagingOptions pagingOptions, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
