using System;

namespace NMSD.Cronus.Core.Multithreading.Work
{
    /// <summary>
    /// Defines async periodical work
    /// </summary>
    public interface IWork
    {
        DateTime ScheduledStart { get; }
        void Start();
    }
}