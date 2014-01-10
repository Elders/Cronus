using System;

namespace NMSD.Cronus.Multithreading.Work
{
    public static class WorkExtensions
    {
        /// <summary>
        /// Calculates if a job is ready to start.The condition is met when the job is behind schedule (ICrawlerJob.ScheduledStart<=DateTime.UtcNow)
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public static bool IsReadyToStart(this IWork job)
        {
            return job.ScheduledStart <= DateTime.UtcNow;
        }
    }
}