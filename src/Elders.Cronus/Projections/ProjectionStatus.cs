using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "27fdcbb1-d334-473a-b62c-70cc3b6ffdfb")]
    public class ProjectionStatus : ValueObject<ProjectionStatus>
    {
        ProjectionStatus() { }

        ProjectionStatus(string status)
        {
            this.status = status;
        }

        [DataMember(Order = 1)]
        string status;

        public static ProjectionStatus Building = new ProjectionStatus("building");

        public static ProjectionStatus Live = new ProjectionStatus("live");

        public static ProjectionStatus Canceled = new ProjectionStatus("canceled");

        public static ProjectionStatus Timedout = new ProjectionStatus("timedout");

        public static ProjectionStatus Create(string status)
        {
            switch (status?.ToLower())
            {
                case "building":
                    return Building;
                case "live":
                    return Live;
                case "canceled":
                    return Canceled;
                case "timedout":
                    return Timedout;
                default:
                    return new ProjectionStatus("unknown");
            }
        }

        public static ProjectionStatus Create(DateTime timestamp)
        {
            return new ProjectionStatus(timestamp.ToFileTimeUtc().ToString());
        }

        public static implicit operator string(ProjectionStatus status)
        {
            if (ReferenceEquals(null, status) == true) throw new ArgumentNullException(nameof(status));
            return status.status;
        }

        public override string ToString()
        {
            return status;
        }
    }


}
