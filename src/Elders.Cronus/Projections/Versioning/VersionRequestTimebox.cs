using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    /// <summary>
    /// Specifies a time frame when a projection rebuild starts and when it expires
    /// </summary>
    [DataContract(Name = "4c8d4c59-cc5a-40f8-9b08-14fcc57f51a9")]
    public class VersionRequestTimebox
    {
        const int OneHour = 3600000;

        const int EightHours = 28800000;

        const int RealyLongTime = 24 * 60 * 60 * 1000; // 24h
        const int Forever = int.MaxValue;

        VersionRequestTimebox() { }

        public VersionRequestTimebox(DateTime requestStartAt) : this(requestStartAt, requestStartAt.AddMilliseconds(Forever)) { }

        public VersionRequestTimebox(DateTime rebuildStartAt, DateTime finishRequestUntil)
        {
            if (rebuildStartAt.AddMinutes(1) >= finishRequestUntil) throw new ArgumentException("Projection rebuild timebox is not realistic.");

            RequestStartAt = rebuildStartAt;
            FinishRequestUntil = finishRequestUntil;
        }

        /// <summary>
        /// The time when a <see cref="VersionRequestTimebox"/> starts
        /// </summary>
        [DataMember(Order = 1)]
        public DateTime RequestStartAt { get; private set; }

        /// <summary>
        /// The time when a <see cref="VersionRequestTimebox"/> expires
        /// </summary>
        [DataMember(Order = 2)]
        public DateTime FinishRequestUntil { get; private set; }

        public VersionRequestTimebox GetNext()
        {
            var newStartAt = DateTime.UtcNow;
            if (newStartAt < FinishRequestUntil)
                newStartAt = FinishRequestUntil;

            return new VersionRequestTimebox(newStartAt);
        }

        public VersionRequestTimebox Reset()
        {
            return new VersionRequestTimebox()
            {
                RequestStartAt = RequestStartAt,
                FinishRequestUntil = DateTime.UtcNow
            };
        }

        public bool HasExpired => FinishRequestUntil < DateTime.UtcNow;

        public override string ToString()
        {
            return $"Version request timebox: Starts at `{RequestStartAt}`. Expires at `{FinishRequestUntil}`";
        }
    }
}
