using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus
{
    public class SafeBatchWithBatchScopeContextFactory<T> : SafeBatchFactory<T, Context>
            where T : IMessage
    {
        private readonly ISafeBatchRetryStrategy<T> retryStrategy;
        public SafeBatchWithBatchScopeContextFactory(ISafeBatchRetryStrategy<T> retryStrategy, ScopeFactory scopeFactory)
        {
            this.ScopeFactory = scopeFactory;
            this.retryStrategy = retryStrategy;
        }

        public ScopeFactory ScopeFactory { get; private set; }

        public override SafeBatch<T, Context> Initialize()
        {
            return new SafeBatch<T, Context>(new BatchScopeContextAware(ScopeFactory.CreateBatchScope), retryStrategy);
        }

        class BatchScopeContextAware : ISafeBatchContextAware<T, Context>
        {
            IBatchScope batchScope = null;

            private readonly Func<IBatchScope> batchScopeFactory;

            Context context = null;

            public BatchScopeContextAware(Func<IBatchScope> batchScopeFactory)
            {
                this.batchScopeFactory = batchScopeFactory;
            }

            public Context OnBatchBeginTry(List<T> items)
            {
                context = new Context();
                batchScope = batchScopeFactory();
                if (batchScope.Context == null)
                    batchScope.Context = new ScopeContext();
                context.BatchScopeContext = batchScope.Context;
                batchScope.Begin();
                return context;
            }

            public void OnBatchEndTry(List<T> items, Context context)
            {
                batchScope.End();
                batchScope.Context.Clear();
                batchScope = null;
            }

        }
    }
}
