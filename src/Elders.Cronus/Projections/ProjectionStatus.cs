using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Elders.Cronus.Projections;

[DataContract(Name = "27fdcbb1-d334-473a-b62c-70cc3b6ffdfb")]
public class ProjectionStatus : IEqualityComparer<ProjectionStatus>, IEquatable<ProjectionStatus>
{
    ProjectionStatus() { }

    ProjectionStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            throw new ArgumentNullException("status");

        Status = status;
    }

    [DataMember(Order = 1)]
    public string Status { get; private set; }

    /// <summary>
    /// The projection does not exist
    /// </summary>
    public static ProjectionStatus NotPresent = new ProjectionStatus("not_present");

    /// <summary>
    /// The projection is currently creating a new <see cref="ProjectionVersion"/>
    /// </summary>
    public static ProjectionStatus New = new ProjectionStatus("new");

    /// <summary>
    /// The projection is currently being fixed
    /// </summary>
    public static ProjectionStatus Fixing = new ProjectionStatus("fixing");

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

    public static ProjectionStatus Paused = new ProjectionStatus("paused");

    public static ProjectionStatus Unknown = new ProjectionStatus("unknown");

    public static ProjectionStatus Create(string status)
    {
        switch (status?.ToLower())
        {
            case "new":
            case "replaying":
            case "building":
                return New;
            case "fixing":
            case "rebuilding":
                return Fixing;
            case "live":
                return Live;
            case "canceled":
                return Canceled;
            case "timedout":
                return Timedout;
            case "paused":
                return Paused;
            case "not_present":
                return NotPresent;
            default:
                return new ProjectionStatus("unknown");
        }
    }

    public static implicit operator string(ProjectionStatus status)
    {
        if (status is null == true) throw new ArgumentNullException(nameof(status));
        return status.Status;
    }

    public bool Equals(ProjectionStatus other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Status == other.Status)
            return true;

        // Everything bellow is for backwords comp
        if (Status == New)
            return other.Status == "replaying" || other.Status == "building";

        if (Status == "replaying")
            return other.Status == New || other.Status == "building";

        if (Status == "building")
            return other.Status == New || other.Status == "replaying";

        if (Status == Fixing)
            return other.Status == "rebuilding";

        if (Status == "rebuilding")
            return other.Status == Fixing;

        return false;
    }

    public bool Equals(ProjectionStatus left, ProjectionStatus right)
    {
        if (left is null && right is null) return true;
        if (left is null)
            return false;
        else
            return left.Equals(right);
    }

    public int GetHashCode([DisallowNull] ProjectionStatus obj)
    {
        return obj.GetHashCode();
    }

    public static bool operator ==(ProjectionStatus left, ProjectionStatus right)
    {
        if (left is null && right is null) return true;
        if (left is null)
            return false;
        else
            return left.Equals(right);
    }

    public static bool operator !=(ProjectionStatus left, ProjectionStatus right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;

        return Equals(obj as ProjectionStatus);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int startValue = 2909;
            int multiplier = 6733;

            int hashCode = startValue;

            hashCode = hashCode * multiplier ^ this.Status.GetHashCode();

            return hashCode;
        }
    }
}
