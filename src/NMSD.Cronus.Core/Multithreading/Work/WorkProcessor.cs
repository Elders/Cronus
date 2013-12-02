using System;
using System.Threading;

namespace NMSD.Cronus.Core.Multithreading.Work
{
    /// <summary>
    /// This class represents a wraper of a thread which does countinous work over a work source
    /// </summary>
    internal class WorkProcessor
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(WorkProcessor));

        string name;

        volatile bool shouldStop = false;

        private Thread thread;

        /// <summary>
        /// Creates an instace of a crawler.
        /// </summary>
        /// <param name="name">Defines the thread name of the crawler instance</param>
        public WorkProcessor(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Starts a dedicated to the crawler's thread.Which countinously takes and executes work.
        /// </summary>
        /// <param name="workSource">Crawler worksource</param>
        public void StartCrawling(WorkSource workSource)
        {
            if (thread == null)
            {
                thread = new Thread(new ThreadStart(() =>
                {
                    while (!shouldStop)
                    {
                        try
                        {
                            log.Debug("Getting available work...");
                            var work = workSource.GetAvailableWork();
                            if (work != null)
                            {
                                log.InfoFormat("Executing work [{0}]", work);
                                work.Start();
                                log.InfoFormat("Work finished successfully. [{0}]", work);
                                workSource.ReturnFinishedWork(work);
                                log.DebugFormat("Work returned to the source. [{0}]", work);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception occured while executing a work. You should take care for all exceptions while you implement 'ICrawlerJob.Start()' method.", ex);
                        }
                    }
                    log.Debug("Crowler was stopped.");
                }));
                thread.Name = name;
                thread.Start();
            }
            else
            {
                log.FatalFormat("Crawler '{0}' is already running on another source.", name);
                throw new InvalidOperationException(String.Format("Crawler '{0}' is already running on another source.", name));
            }
        }

        /// <summary>
        /// Tells to the crawler's thread to finish it's last work and exit.
        /// </summary>
        public void Stop()
        {
            log.DebugFormat("Stopping crawler '{0}'...", name);
            thread = null;
            shouldStop = true;
        }

    }
}