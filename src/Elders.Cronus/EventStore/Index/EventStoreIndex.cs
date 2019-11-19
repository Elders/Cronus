﻿using Elders.Cronus.Logging;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "55f9e248-7bb3-4288-8db8-ba9620c67228")]
    public class EventToAggregateRootId : IEventStoreIndex
    {
        private readonly IIndexStore indexStore;

        public EventToAggregateRootId(IIndexStore indexStore)
        {
            this.indexStore = indexStore;
        }

        public void Index(AggregateCommit aggregateCommit)
        {
            List<IndexRecord> indexRecordsBatch = new List<IndexRecord>();
            foreach (var @event in aggregateCommit.Events)
            {
                string eventTypeId = @event.Unwrap().GetType().GetContractId();
                var record = new IndexRecord(eventTypeId, aggregateCommit.AggregateRootId);
                indexRecordsBatch.Add(record);
            }

            indexStore.Apend(indexRecordsBatch);
        }

        public IEnumerable<IndexRecord> EnumerateRecords(string dataId)
        {
            // TODO: index exists?
            return indexStore.Get(dataId);
        }
    }
}
