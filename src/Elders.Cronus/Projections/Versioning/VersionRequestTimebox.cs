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

        public VersionRequestTimebox(DateTime rebuildStartAt) : this(rebuildStartAt, rebuildStartAt.AddMilliseconds(Forever)) { }

        public VersionRequestTimebox(DateTime rebuildStartAt, DateTime rebuildFinishUntil)
        {
            if (rebuildStartAt.AddMinutes(1) >= rebuildFinishUntil) throw new ArgumentException("Projection rebuild timebox is not realistic.");

            RebuildStartAt = rebuildStartAt;
            RebuildFinishUntil = rebuildFinishUntil;
        }

        /// <summary>
        /// The time when a <see cref="VersionRequestTimebox"/> starts
        /// </summary>
        [DataMember(Order = 1)]
        public DateTime RebuildStartAt { get; private set; }

        /// <summary>
        /// The time when a <see cref="VersionRequestTimebox"/> expires
        /// </summary>
        [DataMember(Order = 2)]
        public DateTime RebuildFinishUntil { get; private set; }

        public VersionRequestTimebox GetNext()
        {
            var newStartAt = DateTime.UtcNow;
            if (newStartAt < RebuildFinishUntil)
                newStartAt = RebuildFinishUntil;

            return new VersionRequestTimebox(newStartAt);
        }

        public VersionRequestTimebox Reset()
        {
            return new VersionRequestTimebox()
            {
                RebuildStartAt = RebuildStartAt,
                RebuildFinishUntil = DateTime.UtcNow
            };
        }

        public bool HasExpired => RebuildFinishUntil < DateTime.UtcNow;

        public override string ToString()
        {
            return $"Version request timebox: Starts at `{RebuildStartAt}`. Expires at `{RebuildFinishUntil}`";
        }
    }
}
