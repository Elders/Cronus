using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "bb4883b9-c3a5-48e5-8ba1-28fb94d061ac")]
    public class ProjectionVersion : ValueObject<ProjectionVersion>
    {
        ProjectionVersion() { }

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

        /// <summary>
        /// Gets the next <see cref="ProjectionVersion"/> which is always with <see cref="ProjectionStatus"/> <see cref="ProjectionStatus.Building"/> and increased Revision.
        /// </summary>
        /// <returns>Returns a <see cref="ProjectionVersion"/></returns>
        public ProjectionVersion NextRevision()
        {
            return new ProjectionVersion(ProjectionName, ProjectionStatus.Building, Revision + 1, Hash);
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
            unchecked
            {
                int hashCode = 5749;
                int multiplier = 7919;
                hashCode = (hashCode * multiplier) ^ Revision.GetHashCode();
                hashCode = (hashCode * multiplier) ^ ProjectionName.GetHashCode();
                hashCode = (hashCode * multiplier) ^ Hash.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ProjectionVersion left, ProjectionVersion right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return true;

            if (ReferenceEquals(null, left))
                return false;

            if (ReferenceEquals(null, right))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(ProjectionVersion left, ProjectionVersion right)
        {
            return !(left == right);
        }

        public static bool operator >(ProjectionVersion left, ProjectionVersion right)
        {
            if (left is null && right is null == false) return false;
            if (left is null && right is null) return false;
            if (left is null == false && right is null) return true;

            if (left.ProjectionName.Equals(right.ProjectionName) == false) throw new ArgumentException($"Unable to compare projection versions. ProjectionNames do not match. {left.ProjectionName} != {right.ProjectionName}");

            return left.Revision > right.Revision;
        }

        public static bool operator >=(ProjectionVersion left, ProjectionVersion right)
        {
            if (left is null && right is null == false) return false;
            if (left is null && right is null) return true;
            if (left is null == false && right is null) return true;

            if (left.ProjectionName.Equals(right.ProjectionName) == false) throw new ArgumentException($"Unable to compare projection versions. ProjectionNames do not match. {left.ProjectionName} != {right.ProjectionName}");

            return left.Revision >= right.Revision;
        }

        public static bool operator <(ProjectionVersion left, ProjectionVersion right)
        {
            if (left is null && right is null == false) return true;
            if (left is null && right is null) return false;
            if (left is null == false && right is null) return false;

            if (left.ProjectionName.Equals(right.ProjectionName) == false) throw new ArgumentException($"Unable to compare projection versions. ProjectionNames do not match. {left.ProjectionName} != {right.ProjectionName}");

            return left.Revision < right.Revision;
        }

        public static bool operator <=(ProjectionVersion left, ProjectionVersion right)
        {
            if (left is null && right is null == false) return true;
            if (left is null && right is null) return true;
            if (left is null == false && right is null) return false;

            if (left.ProjectionName.Equals(right.ProjectionName) == false) throw new ArgumentException($"Unable to compare projection versions. ProjectionNames do not match. {left.ProjectionName} != {right.ProjectionName}");

            return left.Revision <= right.Revision;
        }

        public override string ToString()
        {
            return ProjectionName + "_" + Hash + "_" + Revision + "_" + Status;
        }
    }
}
