using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "4c8d4c59-cc5a-40f8-9b08-14fcc57f51a9")]
    public class VersionRequestTimebox
    {
        const int OneHour = 3600000;

        VersionRequestTimebox() { }

        public VersionRequestTimebox(DateTime rebuildStartAt) : this(rebuildStartAt, rebuildStartAt.AddMilliseconds(70000)) { }

        public VersionRequestTimebox(DateTime rebuildStartAt, DateTime rebuildFinishUntil)
        {
            if (rebuildStartAt.AddMinutes(1) >= rebuildFinishUntil) throw new ArgumentException("Projection rebuild timebox is not realistic.");

            RebuildStartAt = rebuildStartAt;
            RebuildFinishUntil = rebuildFinishUntil;
        }

        [DataMember(Order = 1)]
        public DateTime RebuildStartAt { get; private set; }

        [DataMember(Order = 2)]
        public DateTime RebuildFinishUntil { get; private set; }

        public VersionRequestTimebox GetNext()
        {
            var newStartAt = RebuildFinishUntil.AddMinutes(5);

            return new VersionRequestTimebox(newStartAt);

        }
    }
}
