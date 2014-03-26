using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus
{
    public class SafeBatch<T>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SafeBatch<T>));

        BatchTry<T> batchRetry;

        public SafeBatch()
            : this(new DefaultRetryStrategy<T>()) { }

        public SafeBatch(ISafeBatchRetryStrategy<T> batchRetryStrategy)
        {
            if (batchRetryStrategy == null)
                throw new ArgumentNullException("batchRetryStrategy");

            this.batchRetry = new BatchTry<T>(batchRetryStrategy);
        }

        public SafeBatchResult<T> Execute(Action<T, Context> itemAction, List<T> items, Func<Context> beginScope = null, Action endScope = null)
        {
            return batchRetry.Try(itemAction, items, beginScope, endScope);
        }

        class BatchTry<T>
        {
            private readonly ISafeBatchRetryStrategy<T> retryStrategy;

            public BatchTry(ISafeBatchRetryStrategy<T> retryStrategy)
            {
                this.retryStrategy = retryStrategy;
            }

            public SafeBatchResult<T> Try(Action<T, Context> itemSource, List<T> itemsToTry, Func<Context> beginScope = null, Action endScope = null)
            {
                List<T> totalSuccess = new List<T>();
                List<T> totalFailed = new List<T>();

                List<T> successItems = new List<T>();
                List<List<T>> failed = new List<List<T>>();
                failed.Add(new List<T>(itemsToTry));
                bool firstRun = true;
                int retryCount = 0;
                while (failed.Count > 0)
                {
                    if (retryCount > 2 && retryStrategy is NoRetryStrategy<T>)
                        throw new Exception("NoRetryStrategy has a bug");

                    if (retryCount > itemsToTry.Count)
                        throw new Exception("Infinite loop in safe batch retry");

                    retryCount++;
                    List<List<T>> itemsToRetry = new List<List<T>>(failed);

                    successItems.Clear();
                    failed.Clear();
                    retryStrategy.Retry(firstRun, (batch) =>
                    {
                        var context = beginScope();
                        batch.ForEach(item => itemSource(item, context));
                        //itemSource.Finish(batch);
                        if (endScope != null) endScope();
                    },
                    itemsToRetry, out successItems, out failed);
                    totalSuccess.AddRange(successItems);

                    if (failed.Count > 0 && log.IsWarnEnabled)
                    {
                        StringBuilder warnInfo = new StringBuilder();
                        warnInfo.AppendLine("Safe batch executtion finished with errors. The failing messages will be retried.");
                        warnInfo.AppendLine("Details: TryCount: " + retryCount);
                        failed.ForEach(failedBatches => failedBatches.ForEach(failedMessage => warnInfo.AppendLine(failedMessage.ToString())));
                        log.Warn(warnInfo.ToString());
                    }

                    if (firstRun && successItems.Count == 0)
                    {
                        firstRun = false;
                        continue;
                    }
                    else
                    {
                        bool hasNoChanceToGetMoreSuccessItems = successItems.Count == 0 && itemsToRetry.Count == failed.Count && itemsToRetry.Sum(x => x.Count) == failed.Sum(x => x.Count);
                        bool allSucceeded = itemsToTry.Count == totalSuccess.Count;
                        if (hasNoChanceToGetMoreSuccessItems || allSucceeded)
                        {
                            totalFailed.AddRange(failed.SelectMany(x => x));
                            break;
                        }
                    }
                    successItems.Clear();
                }
                return new SafeBatchResult<T>(totalSuccess, totalFailed);
            }
        }

        public class DefaultRetryStrategy<T> : ISafeBatchRetryStrategy<T>
        {
            public void Retry(bool isFirstTry, Action<List<T>> batchExecute, List<List<T>> batchesToRetry, out List<T> successItems, out List<List<T>> failedBatches)
            {
                List<List<T>> safeList = new List<List<T>>(batchesToRetry);
                int splitCount = isFirstTry ? 1 : 2;
                successItems = new List<T>();
                failedBatches = new List<List<T>>();
                foreach (var batch in safeList)
                {
                    var splitted = batch.Split(splitCount);
                    foreach (var splittedBatch in splitted)
                    {
                        try
                        {
                            batchExecute(splittedBatch.ToList());
                            successItems.AddRange(splittedBatch);
                        }
                        catch (Exception ex)
                        {
                            var failing = splittedBatch.ToList();
                            failedBatches.Add(failing);
                        }
                    }
                }
            }
        }

        public class NoRetryStrategy<T> : ISafeBatchRetryStrategy<T>
        {
            public void Retry(bool isFirstTry, Action<List<T>> batchExecute, List<List<T>> batchesToRetry, out List<T> successItems, out List<List<T>> failedBatches)
            {
                List<List<T>> safeList = new List<List<T>>(batchesToRetry);
                successItems = new List<T>();
                failedBatches = new List<List<T>>();

                if (!isFirstTry)
                {
                    failedBatches = batchesToRetry;
                    return;
                }
                try
                {
                    var batch = safeList.Single().ToList();
                    batchExecute(batch);
                    successItems.AddRange(batch);

                }
                catch (Exception)
                {
                    failedBatches = batchesToRetry;
                }
            }
        }

    }

    public interface ISafeBatchRetryStrategy<T>
    {
        /// <summary>
        /// Retries the execution of items within a batch.
        /// </summary>
        /// <param name="batchExecute">Executes all action's for items within a batch.</param>
        /// <param name="batchesToRetry">Two dimensional structure holding batches to retry.</param>
        /// <param name="successItems">A list of all successful items within the batches defined in 'batchesToRetry'</param>
        /// <param name="failedBatches">Two dimensional structure holding all failed batches.</param>
        /// <example>  
        /// This sample shows how to retry a batch by splitting it into two smaller batches.
        /// <code> 
        ///     public class DefaultRetryStrategy : ISafeBatchRetryStrategy
        ///     {
        ///         public void Retry(bool isFirstTry, Action<List<T>> batchExecute, List<List<T>> batchesToRetry, out List<T> successItems, out List<List<T>> failedBatches)
        ///         {
        ///             int splitCount = isFirstTry ? 2 : 1;
        ///             successItems = new List<T>();
        ///             failedBatches = new List<List<T>>();
        ///             foreach (var batch in batchesToRetry)
        ///             {
        ///                 foreach (var splittedBatch in batch.Split(splitCount))
        ///                 {
        ///                     try
        ///                     {
        ///                         batchExecute(splittedBatch.ToList());
        ///                         successItems.AddRange(splittedBatch);
        ///                     }
        ///                     catch (Exception)
        ///                     {
        ///                         failedBatches.Add(splittedBatch.ToList());
        ///                     }
        ///                 }
        ///             }
        ///         }
        ///     }
        /// </code> 
        /// </example> 
        void Retry(bool isFirstTry, Action<List<T>> batchExecute, List<List<T>> batchesToRetry, out List<T> successItems, out List<List<T>> failedBatches);
    }

    public class SafeBatchResult<T>
    {
        public List<T> SuccessItems { get; private set; }
        public List<T> FailedItems { get; private set; }

        public SafeBatchResult(List<T> successItems, List<T> failedItems)
        {
            this.SuccessItems = successItems;
            this.FailedItems = failedItems;
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
    }
}