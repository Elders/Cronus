using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "bb4883b9-c3a5-48e5-8ba1-28fb94d061ac")]
    public class ProjectionVersion : ValueObject<ProjectionVersion>
    {
        public ProjectionVersion(string projectionName, ProjectionStatus status, int revision, string hash)
        {
            ProjectionName = projectionName;
            Status = status;
            Revision = revision;
            Hash = hash;
        }

        [DataMember(Order = 1)]
        public string ProjectionName { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionStatus Status { get; private set; }

        [DataMember(Order = 3)]
        public int Revision { get; private set; }

        [DataMember(Order = 4)]
        public string Hash { get; private set; }

        public ProjectionVersion WithStatus(ProjectionStatus status)
        {
            return new ProjectionVersion(ProjectionName, status, Revision, Hash);
        }

        public ProjectionVersion NextRevision()
        {
            return new ProjectionVersion(ProjectionName, Status, Revision + 1, Hash);
        }

        public override bool Equals(ProjectionVersion other)
        {
            if (ReferenceEquals(null, other)) return false;

            return
                string.Equals(Hash, other.Hash, System.StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ProjectionName, other.ProjectionName, System.StringComparison.OrdinalIgnoreCase) &&
                Revision == other.Revision &&
                Status.Equals(other.Status);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Revision.GetHashCode() ^ ProjectionName.GetHashCode() ^ Hash.GetHashCode();
        }

        public static bool operator ==(ProjectionVersion left, ProjectionVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProjectionVersion left, ProjectionVersion right)
        {
            return (left == right) == false;
        }

        public static bool operator >(ProjectionVersion left, ProjectionVersion right)
        {
            if (ReferenceEquals(null, left)) throw new ArgumentNullException(nameof(left));
            if (ReferenceEquals(null, right)) throw new ArgumentNullException(nameof(right));
            if (left.ProjectionName.Equals(right.ProjectionName) == false) throw new ArgumentException($"Unable to compare projection versions. ProjectionNames do not match. {left.ProjectionName} != {right.ProjectionName}");
            if (left.Hash.Equals(right.Hash) == false) throw new ArgumentException($"Unable to compare projection versions. ProjectionHashes do not match. {left.Hash} != {right.Hash}");

            return left.Revision > right.Revision;
        }

        public static bool operator <(ProjectionVersion left, ProjectionVersion right)
        {
            if (ReferenceEquals(null, left)) throw new ArgumentNullException(nameof(left));
            if (ReferenceEquals(null, right)) throw new ArgumentNullException(nameof(right));
            if (left.ProjectionName.Equals(right.ProjectionName) == false) throw new ArgumentException($"Unable to compare projection versions. ProjectionNames do not match. {left.ProjectionName} != {right.ProjectionName}");
            if (left.Hash.Equals(right.Hash) == false) throw new ArgumentException($"Unable to compare projection versions. ProjectionHashes do not match. {left.Hash} != {right.Hash}");

            return left.Revision < right.Revision;
        }

        public override string ToString()
        {
            return ProjectionName + "_" + Hash + "_" + Revision + "_" + Status;
        }
    }
}
