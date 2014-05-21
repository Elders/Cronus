using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elders.Cronus
{
    public abstract class SafeBatchFactory<T, C>
    {
        public abstract SafeBatch<T, C> Initialize();
    }

    public interface ISafeBatchContextAware<T, C>
    {
        C OnBatchBeginTry(List<T> items);
        void OnBatchEndTry(List<T> items, C context);
    }

    public class SafeBatch<T, C>
    {
        private readonly ISafeBatchContextAware<T, C> contextAware;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SafeBatch<T, C>));

        BatchTry<T> batchRetry;

        public SafeBatch(ISafeBatchContextAware<T, C> contextAware)
            : this(contextAware, new DefaultRetryStrategy<T>())
        { }

        protected SafeBatch(ISafeBatchContextAware<T, C> contextAware, ISafeBatchRetryStrategy<T> batchRetryStrategy)
        {
            this.contextAware = contextAware;
            if (batchRetryStrategy == null)
                throw new ArgumentNullException(@"batchRetryStrategy");

            this.batchRetry = new BatchTry<T>(batchRetryStrategy);
        }

        public SafeBatchResult<T> Execute(List<T> items)
        {
            return Execute(items, null);
        }

        public SafeBatchResult<T> Execute(List<T> items, Action<T, C> singleItemAction)
        {
            return batchRetry.Try(items, singleItemAction, contextAware);
        }

        class BatchTry<T>
        {
            private readonly ISafeBatchRetryStrategy<T> retryStrategy;

            public BatchTry(ISafeBatchRetryStrategy<T> retryStrategy)
            {
                this.retryStrategy = retryStrategy;
            }

            public SafeBatchResult<T> Try(List<T> itemsToTry, Action<T, C> singleItemAction, ISafeBatchContextAware<T, C> contextAware)
            {
                bool noRetryOnFailure = itemsToTry.Count == 1;

                List<T> totalSuccess = new List<T>();
                List<TryBatch<T>> totalFailed = new List<TryBatch<T>>();

                List<T> successItems = new List<T>();
                List<TryBatch<T>> failed = new List<TryBatch<T>>();
                failed.Add(new TryBatch<T>(itemsToTry));
                bool firstRun = true;
                int tryCount = 0;
                while (failed.Count > 0)
                {
                    if (tryCount > 1 && noRetryOnFailure) throw new Exception("NoRetryStrategy has a bug. Number of retries > number of items.");
                    if (tryCount > itemsToTry.Count) throw new Exception("Infinite loop in safe batch retry");

                    tryCount++;
                    firstRun = tryCount == 1;
                    List<TryBatch<T>> itemsToRetry = new List<TryBatch<T>>(failed);

                    successItems.Clear();
                    failed.Clear();
                    retryStrategy.Retry(tryCount, (batch) =>
                    {
                        var context = contextAware.OnBatchBeginTry(batch.Items);
                        if (singleItemAction != null)
                            batch.Items.ForEach(item => singleItemAction(item, context));
                        contextAware.OnBatchEndTry(batch.Items, context);
                    },
                    itemsToRetry, out successItems, out failed);
                    totalSuccess.AddRange(successItems);

                    if (failed.Count > 0 && log.IsWarnEnabled)
                    {
                        foreach (var failedBatch in failed)
                        {
                            StringBuilder warnInfo = new StringBuilder();
                            warnInfo.AppendLine("Safe batch executtion finished with errors. The failing messages will be retried.");
                            warnInfo.AppendLine("Details: TryCount: " + tryCount);
                            failedBatch.Items.ForEach(failedMessage => warnInfo.AppendLine(failedMessage.ToString()));
                            log.Warn(warnInfo.ToString(), failedBatch.Error);
                        }
                    }

                    if (firstRun && successItems.Count == 0 && !noRetryOnFailure)
                    {
                        firstRun = false;
                        continue;
                    }
                    else
                    {
                        bool hasNoChanceToGetMoreSuccessItems = successItems.Count == 0 && itemsToRetry.Count == failed.Count && itemsToRetry.Sum(x => x.Items.Count) == failed.Sum(x => x.Items.Count);
                        bool allSucceeded = itemsToTry.Count == totalSuccess.Count;
                        if (noRetryOnFailure || hasNoChanceToGetMoreSuccessItems || allSucceeded)
                        {
                            totalFailed.AddRange(failed);
                            break;
                        }
                    }
                    successItems.Clear();
                }
                SafeBatchResult<T> batchResults = new SafeBatchResult<T>(totalSuccess, totalFailed);
                log.DebugFormat("SafeBatch finished with {0} success items and {1} failing items", totalSuccess.Count, totalFailed.Count);
                return batchResults;
            }
        }
    }

    public class DefaultRetryStrategy<T> : ISafeBatchRetryStrategy<T>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DefaultRetryStrategy<T>));

        public void Retry(int tryCount, Action<TryBatch<T>> batchExecute, List<TryBatch<T>> batchesToRetry, out List<T> successItems, out List<TryBatch<T>> failedBatches)
        {
            List<TryBatch<T>> safeList = new List<TryBatch<T>>(batchesToRetry);
            int splitCount = tryCount == 1 ? 1 : 2;
            successItems = new List<T>();
            failedBatches = new List<TryBatch<T>>();
            foreach (var batch in safeList)
            {
                var splittedBatches = batch.Split(splitCount);
                foreach (var splittedBatch in splittedBatches)
                {
                    try
                    {
                        batchExecute(splittedBatch);
                        successItems.AddRange(splittedBatch.Items);
                    }
                    catch (Exception ex)
                    {
                        log.Warn(splittedBatch.Items, ex);
                        splittedBatch.MarkAsFailed(ex);
                        failedBatches.Add(splittedBatch);
                    }
                }
            }
        }
    }

    public interface ISafeBatchRetryStrategy<T>
    {
        void Retry(int tryCount, Action<TryBatch<T>> batchExecute, List<TryBatch<T>> batchesToRetry, out List<T> successItems, out List<TryBatch<T>> failedBatches);
    }

    public class TryBatch<T>
    {
        public TryBatch(List<T> items, Exception error = null)
        {
            Items = new List<T>(items);
            Error = error;
        }

        public TryBatch(TryBatch<T> batch, Exception error = null)
        {
            Items = new List<T>(batch.Items);
            Error = error;
        }

        public List<T> Items { get; private set; }
        public Exception Error { get; private set; }

        public void MarkAsFailed(Exception error)
        {
            Error = error;
        }
    }

    public interface ISafeBatchResult<T>
    {
        IEnumerable<T> SuccessItems { get; }
        IEnumerable<TryBatch<T>> FailedBatches { get; }
    }

    public class SafeBatchResult<T> : ISafeBatchResult<T>
    {
        public IEnumerable<T> SuccessItems { get; private set; }
        public IEnumerable<TryBatch<T>> FailedBatches { get; private set; }

        public SafeBatchResult(IEnumerable<T> successItems, IEnumerable<TryBatch<T>> failedBatches)
        {
            this.SuccessItems = new List<T>(successItems);
            this.FailedBatches = new List<TryBatch<T>>(failedBatches);
        }
    }

    static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }

        public static IEnumerable<TryBatch<T>> Split<T>(this TryBatch<T> tryBatch, int parts)
        {
            int i = 0;
            var splits = from item in tryBatch.Items
                         group item by i++ % parts into part
                         select new TryBatch<T>(part.ToList());
            return splits;
        }
    }
}