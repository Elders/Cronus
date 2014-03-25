using System.Collections.Concurrent;
using System.Threading;

namespace Elders.Cronus.Multithreading.Work
{
    internal class WorkSource
    {
        ConcurrentQueue<IWork> availableWork;

        ConcurrentQueue<ManualResetEvent> freeCrawlers;

        volatile bool IsSourceReleased = false;

        WorkSourceScheduler workSourceScheduler;

        public WorkSource()
        {
            availableWork = new ConcurrentQueue<IWork>();
            freeCrawlers = new ConcurrentQueue<ManualResetEvent>();
            workSourceScheduler = new WorkSourceScheduler();
            workSourceScheduler.OnWorkReadyForProcess(x =>
            {
                availableWork.Enqueue(x);
                NotifyFreeCrawler();
            });
            workSourceScheduler.StartManage();
        }

        /// <summary>
        /// Returns available <see cref="ICrawlerWork"/> or blocks the thread unitl work is available.
        /// </summary>
        /// <returns><see cref="ICrawlerWork"/> or null if the source is released.</returns>
        public IWork GetAvailableWork()
        {
            IWork work;
            while (!IsSourceReleased)
            {
                if (availableWork.TryDequeue(out work))
                    return work;
                else
                {
                    var handle = new ManualResetEvent(false);
                    freeCrawlers.Enqueue(handle);
                    handle.WaitOne(5000);   // Wait 5 seconds
                }
            }
            return null;
        }

        /// <summary>
        /// Registers/Adds work to the <see cref="CrawlerWorkSource"/>.
        /// </summary>
        /// <param name="work"><see cref="ICrawlerWork"/></param>
        public void RegisterWork(IWork work)
        {
            if (work.IsReadyToStart())
            {
                availableWork.Enqueue(work);
            }
            else
            {
                workSourceScheduler.ScheduleWork(work);
            }
        }

        /// <summary>
        /// Releases all the work from the current <see cref="CrawlerWorkSource"/>,making it unavailable.
        /// </summary>
        public void DisposeSource()
        {
            IsSourceReleased = true;

            if (workSourceScheduler != null)
            {
                workSourceScheduler.Stop();
                workSourceScheduler = null;
            }

            ManualResetEvent handle;
            while (freeCrawlers.TryDequeue(out handle))
            {
                handle.Set();
            }

            availableWork = new ConcurrentQueue<IWork>();
        }

        /// <summary>
        /// Returns <see cref="ICrawlerWork"/> to the current <see cref="CrawlerWorkSource"/> for rescheduling.
        /// </summary>
        /// <param name="work"></param>
        public void ReturnFinishedWork(IWork work)
        {
            if (!IsSourceReleased)
            {
                if (work.IsReadyToStart())
                {
                    availableWork.Enqueue(work);
                    NotifyFreeCrawler();
                }
                else
                {
                    workSourceScheduler.ScheduleWork(work);
                }
            }
        }

        private void NotifyFreeCrawler()
        {
            if (!freeCrawlers.IsEmpty)
            {
                ManualResetEvent handle;
                freeCrawlers.TryDequeue(out handle);
                handle.Set();
            }
        }
    }
}