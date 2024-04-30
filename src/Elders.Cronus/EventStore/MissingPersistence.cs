using Elders.Cronus.EventStore.Index;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

internal sealed class MissingPersistence : IEventStoreFactory, IEventStore, IMessageCounter, IEventStorePlayer, IIndexStore
{
    private const string MissingPersistenceMessage = "The Persistence is not configured. Please install a nuget package which provides persistence capabilities such as IEventStoreFactory, IEventStore, IMessageCounter, IEventStorePlayer, IIndexStore. ex.: Cronus.Persistence.Cassandra. You can disable the persistence functionality with Cronus:ApplicationServicesEnabled = false";

    public Task ApendAsync(IndexRecord indexRecord) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task AppendAsync(AggregateCommit aggregateCommit) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task AppendAsync(AggregateEventRaw eventRaw) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task DecrementAsync(Type messageType, long decrementWith = 1) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task DeleteAsync(IndexRecord indexRecord) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task<bool> DeleteAsync(AggregateEventRaw eventRaw) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task EnumerateEventStore(PlayerOperator @operator, PlayerOptions replayOptions) => throw new NotImplementedException(MissingPersistenceMessage);

    public IAsyncEnumerable<IndexRecord> GetAsync(string indexRecordId) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task<LoadIndexRecordsResult> GetAsync(string indexRecordId, string paginationToken, int pageSize) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task<long> GetCountAsync(Type messageType) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task<long> GetCountAsync(string indexRecordId) => throw new NotImplementedException(MissingPersistenceMessage);

    public IEventStore GetEventStore() => throw new System.NotImplementedException(MissingPersistenceMessage);

    public Task IncrementAsync(Type messageType, long incrementWith = 1) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task<EventStream> LoadAsync(IBlobId aggregateId) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingAsync(IBlobId aggregateId, PagingOptions pagingOptions) => throw new NotImplementedException(MissingPersistenceMessage);

    public Task ResetAsync(Type messageType) => throw new NotImplementedException(MissingPersistenceMessage);
}
