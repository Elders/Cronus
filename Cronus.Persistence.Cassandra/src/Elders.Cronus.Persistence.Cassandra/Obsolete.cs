//using System;
//using System.Collections.Generic;
//using Elders.Cronus.EventSourcing;

//namespace Elders.Cronus.Persistence.Cassandra
//{
//    public class CassandraPersisterContext : IEventStoreBatchContext
//    {

//    }

//    public class EventStoreSafeBatcContextFactory : SafeBatchFactory<DomainMessageCommit, IEventStoreBatchContext>
//    {
//        private readonly IEventStorePersister persister;
//        private readonly ISafeBatchRetryStrategy<DomainMessageCommit> retryStrategy;
//        public EventStoreSafeBatcContextFactory(ISafeBatchRetryStrategy<DomainMessageCommit> retryStrategy, IEventStorePersister persister)
//        {
//            this.persister = persister;
//            this.retryStrategy = retryStrategy;
//        }

//        public override SafeBatch<DomainMessageCommit, IEventStoreBatchContext> Initialize()
//        {
//            return new SafeBatch<DomainMessageCommit, IEventStoreBatchContext>(new CassandraPersisterContextAware(persister), retryStrategy);
//        }

//        class CassandraPersisterContextAware : ISafeBatchContextAware<DomainMessageCommit, IEventStoreBatchContext>
//        {
//            private readonly IEventStorePersister persister;
//            IEventStoreBatchContext context = null;
//            public CassandraPersisterContextAware(IEventStorePersister persister)
//            {
//                this.persister = persister;
//            }

//            public IEventStoreBatchContext OnBatchBeginTry(List<DomainMessageCommit> items)
//            {
//                return context;
//            }

//            public void OnBatchEndTry(List<DomainMessageCommit> items, IEventStoreBatchContext context)
//            {
//                persister.Persist(items);
//            }

//        }
//    }
//}
