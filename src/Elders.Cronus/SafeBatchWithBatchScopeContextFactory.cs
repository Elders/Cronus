using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus
{
    public class SafeBatchWithBatchUnitOfWorkContextFactory<T> : SafeBatchFactory<T, Context>
            where T : TransportMessage
    {
        public SafeBatchWithBatchUnitOfWorkContextFactory(UnitOfWorkFactory unitOfWorkFactory)
        {
            this.UnitOfWorkFactory = unitOfWorkFactory;
        }

        public UnitOfWorkFactory UnitOfWorkFactory { get; private set; }

        public override SafeBatch<T, Context> CreateSafeBatch()
        {
            return new SafeBatch<T, Context>(new BatchUnitOfWorkContextAware(UnitOfWorkFactory.CreateBatchUnitOfWork));
        }

        class BatchUnitOfWorkContextAware : ISafeBatchContextAware<T, Context>
        {
            IBatchUnitOfWork batchUnitOfWork = null;

            private readonly Func<IBatchUnitOfWork> batchUnitOfWorkFactory;

            Context context = null;

            public BatchUnitOfWorkContextAware(Func<IBatchUnitOfWork> batchUnitOfWorkFactory)
            {
                this.batchUnitOfWorkFactory = batchUnitOfWorkFactory;
            }

            public Context OnBeginTry(List<T> items)
            {
                context = new Context();
                batchUnitOfWork = batchUnitOfWorkFactory();
                if (batchUnitOfWork.Context == null)
                    batchUnitOfWork.Context = new UnitOfWorkContext();
                context.BatchContext = batchUnitOfWork.Context;
                batchUnitOfWork.Begin();
                return context;
            }

            public void OnEndTry(List<T> items, Context context)
            {
                batchUnitOfWork.End();
                batchUnitOfWork.Context.Clear();
                batchUnitOfWork = null;
            }

        }
    }
}
