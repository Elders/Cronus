using System;
using System.Collections.Generic;

namespace Elders.Cronus.Multithreading.Work
{
    public class WorkPool
    {
        private List<WorkProcessor> crawlers;

        int jobsInSource = 0;

        string poolName;

        int workersCount;

        WorkSource workSource;

        /// <summary>
        /// Creates and instance of the CrawlerWorkPool
        /// </summary>
        /// <param name="poolName">Pool name</param>
        /// <param name="numberOfCrawlers">Specifies the number of crawlers to serve the pool</param>
        public WorkPool(string poolName, int numberOfCrawlers)
        {
            this.poolName = poolName;
            workSource = new WorkSource();
            workersCount = numberOfCrawlers;
            crawlers = new List<WorkProcessor>();
        }

        /// <summary>
        /// Adds work to the pool's <see cref="CrawlerWorkSource"/> 
        /// </summary>
        /// <param name="job"></param>
        public void AddWork(IWork job)
        {
            workSource.RegisterWork(job);
            jobsInSource++;
        }

        /// <summary>
        /// Starts the pool's crawlers
        /// </summary>
        public void StartCrawlers()
        {
            for (int i = 1; i <= workersCount && i <= jobsInSource; i++)
            {
                WorkProcessor crw = new WorkProcessor(String.Format("Pool: '{0}' \t Crawler: '{1}'", poolName, i));
                crw.StartCrawling(workSource);
                crawlers.Add(crw);
            }
        }

        /// <summary>
        /// Stops the pool's crawlers
        /// </summary>
        public void Stop()
        {
            foreach (var crawler in crawlers)
            {
                crawler.Stop();
            }
            crawlers.Clear();
            if (workSource != null)
            {
                workSource.DisposeSource();
                workSource = null;
            }
        }

    }
}
