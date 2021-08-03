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

        /// <summary>
        /// The projection does not exist
        /// </summary>
        public static ProjectionStatus NotPresent = new ProjectionStatus("not_present");

        /// <summary>
        /// This is for backwards compat.
        /// </summary>
        internal static ProjectionStatus Building = new ProjectionStatus("building");

        /// <summary>
        /// The projection is currently replaying
        /// </summary>
        public static ProjectionStatus Replaying = new ProjectionStatus("replaying");

        /// <summary>
        /// The projection is currently rebuilding
        /// </summary>
        public static ProjectionStatus Rebuilding = new ProjectionStatus("rebuilding");

        /// <summary>
        /// The projection is rebuilt and ready for use
        /// </summary>
        public static ProjectionStatus Live = new ProjectionStatus("live");

        /// <summary>
        /// The projection rebuild is canceled by a user or an error happened during rebuild. Check the reason for more details
        /// </summary>
        public static ProjectionStatus Canceled = new ProjectionStatus("canceled");

        /// <summary>
        /// The projection rebuild has timed out because the rebuild process went beyond the allowed <see cref="Elders.Cronus.Projections.Versioning.VersionRequestTimebox"/>
        /// </summary>
        public static ProjectionStatus Timedout = new ProjectionStatus("timedout");



        public static ProjectionStatus Create(string status)
        {
            switch (status?.ToLower())
            {
                case "building":
                case "replaying":
                    return Replaying;
                case "rebuilding":
                    return Rebuilding;
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
