using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction
{
    public class DefaultAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        private IRevisionStore revisionStore;

        private IAggregateRootLock aggregateRootLock;

        public DefaultAggregateRootAtomicAction(IAggregateRootLock aggregateRootLock, IRevisionStore revisionStore)
        {
            this.aggregateRootLock = aggregateRootLock;
            this.revisionStore = revisionStore;
        }

        public Result<bool> Execute(IAggregateRootId arId, int aggregateRootRevision, Action action)
        {
            var lockResult = Lock(arId, TimeSpan.FromSeconds(1));
            if (lockResult.IsNotSuccessful)
                return Result.Error("lock failed");

            try
            {
                if (CanExecuteAction(arId, aggregateRootRevision))
                {
                    var actionResult = ExecuteAction(action);

                    if (actionResult.IsNotSuccessful)
                    {
                        Rollback(arId, aggregateRootRevision - 1);//????

                        return Result.Error("action failed");
                    }

                    return actionResult;
                }

                return Result.Error("unable to execute action");
            }
            catch (Exception ex)
            {
                // TODO log
                return Result.Error(ex);
            }
            finally
            {
                Unlock(lockResult.Value);
            }
        }

        private Result<object> Lock(IAggregateRootId arId, TimeSpan ttl)
        {
            object mutex;

            try
            {
                mutex = aggregateRootLock.Lock(arId, ttl);

                if (ReferenceEquals(null, mutex) == false)
                    return new Result<object>().WithError("failed lock");

                return new Result<object>(mutex);
            }
            catch (Exception ex)
            {
                return new Result<object>().WithError(ex);
            }
        }

        private Result<bool> CheckForExistingRevision(IAggregateRootId arId)
        {
            return revisionStore.HasRevision(arId);
        }

        private Result<bool> SavePreviouseRevison(IAggregateRootId arId, int revision)
        {
            return revisionStore.SaveRevision(arId, revision - 1);
        }

        private bool IsConsecutiveRevision(IAggregateRootId arId, int revision)
        {
            var storedRevisionResult = revisionStore.GetRevision(arId);
            return storedRevisionResult.IsSuccessful && storedRevisionResult.Value == revision - 1;
        }

        private Result<bool> IncrementRevision(IAggregateRootId arId, int newRevision)
        {
            return revisionStore.SaveRevision(arId, newRevision);
        }

        private Result<bool> ExecuteAction(Action action)
        {
            try
            {
                action();
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Error(ex);
            }
        }

        private void Rollback(IAggregateRootId arId, int revision)
        {
            revisionStore.SaveRevision(arId, revision - 1);
        }

        private void Unlock(object mutex)
        {
            if (ReferenceEquals(null, mutex)) return;

            try
            {
                aggregateRootLock.Unlock(mutex);
            }
            catch (Exception ex)
            {
                // TODO: log
            }
        }
        private bool CanExecuteAction(IAggregateRootId arId, int aggregateRootRevision)
        {
            try
            {
                var existingRevisionResult = CheckForExistingRevision(arId);

                if (existingRevisionResult.IsNotSuccessful)
                {
                    var prevRevResult = SavePreviouseRevison(arId, aggregateRootRevision);

                    if (prevRevResult.IsNotSuccessful)
                        return false;
                }

                var idConsecutiveRevision = IsConsecutiveRevision(arId, aggregateRootRevision);

                if (idConsecutiveRevision)
                {
                    return IncrementRevision(arId, aggregateRootRevision).IsSuccessful;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
        }
    }
}