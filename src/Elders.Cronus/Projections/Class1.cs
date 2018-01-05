using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Projections
{
    public class ProjectionAR : AggregateRoot<ProjectionArState>
    {
        public void Replay()
        {
            if (CanReplay())
            {
                var projectionVersion = new ProjectionVersion(state.ProjectionName, ProjectionStatus.Building, state.NextVersionNumber);
                var @event = new ReplayProjectionStarted(state.Id, projectionVersion);
                Apply(@event);
            }
        }

        public void CancelReplay()
        {
            if (CanCancel())
            {
                var projectionVersion = state.Versions.Where(x => x.Status == ProjectionStatus.Building).Single();
                var @event = new ReplayProjectionCanceled(state.Id, projectionVersion);
                Apply(@event);
            }
        }

        bool CanCancel()
        {
            return state.Versions.Any(x => x.Status == ProjectionStatus.Building);
        }

        bool CanReplay()
        {
            return state.Versions.Any(x => x.Status == ProjectionStatus.Building) == false;
        }
    }

    public class ProjectionArState : AggregateRootState<ProjectionAR, ProjectionArId>
    {
        public ProjectionArState()
        {
            Versions = new HashSet<ProjectionVersion>();
        }

        public override ProjectionArId Id { get; set; }

        public HashSet<ProjectionVersion> Versions { get; set; }

        public ProjectionVersion Live { get { return Versions.Where(x => x.Status == ProjectionStatus.Live).SingleOrDefault(); } }

        public string ProjectionName { get { return Versions.First().ProjectionName; } }

        public ProjectionVersion LatestVersion { get { return Versions.Where(x => x.VersionNumber == Versions.Max(ver => ver.VersionNumber)).SingleOrDefault(); } }

        public int NextVersionNumber { get { return LatestVersion.VersionNumber + 1; } }

        public void When(ReplayProjectionCanceled e)
        {
            if (Versions.Remove(e.ProjectionVersion))
            {
                var canceledVersion = e.ProjectionVersion.WithStatus(ProjectionStatus.Canceled);
                Versions.Add(canceledVersion);
            }
        }

        public void When(ReplayProjectionStarted e)
        {
            Versions.Add(e.ProjectionVersion);
        }

        public void When(ReplayProjectionFinished e)
        {
            if (Versions.RemoveWhere(x => x.Status == ProjectionStatus.Building) > 0)
            {
                Versions.Add(e.ProjectionVersion);
            }
        }
    }

    [DataContract(Name = "bb4883b9-c3a5-48e5-8ba1-28fb94d061ac")]
    public class ProjectionVersion : ValueObject<ProjectionVersion>
    {
        public ProjectionVersion(string projectionName, ProjectionStatus status, int versionNumber)
        {
            ProjectionName = projectionName;
            Status = status;
            VersionNumber = versionNumber;
        }

        [DataMember(Order = 1)]
        public string ProjectionName { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionStatus Status { get; private set; }

        [DataMember(Order = 3)]
        public int VersionNumber { get; private set; }

        public ProjectionVersion WithStatus(ProjectionStatus status)
        {
            return new ProjectionVersion(ProjectionName, status, VersionNumber);
        }

        public override bool Equals(ProjectionVersion other)
        {
            return VersionNumber == other.VersionNumber;
        }

        public override int GetHashCode()
        {
            return VersionNumber.GetHashCode();
        }
    }

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
                default:
                    throw new NotSupportedException();
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

    [DataContract(Name = "a3694e4d-7642-4d83-a468-b80cf625fda2")]
    public class ReplayProjection : ICommand
    {
        ReplayProjection() { }

        public ReplayProjection(ProjectionArId id, ProjectionVersion projectionVersion)
        {
            Id = id;
            ProjectionVersion = projectionVersion;
        }

        [DataMember(Order = 1)]
        public ProjectionArId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }
    }

    [DataContract(Name = "3ed5d535-d937-4b25-ae90-fb59408b38cc")]
    public class CancelReplayProjection : ICommand
    {
        CancelReplayProjection()
        { }

        public CancelReplayProjection(ProjectionArId id, ProjectionVersion projectionVersion)
        {
            Id = id;
            ProjectionVersion = projectionVersion;
        }

        [DataMember(Order = 1)]
        public ProjectionArId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }
    }

    [DataContract(Name = "5788a757-5dd6-4680-8f24-add1dfa7539b")]
    public class ReplayProjectionStarted : IEvent
    {
        ReplayProjectionStarted() { }

        public ReplayProjectionStarted(ProjectionArId id, ProjectionVersion projectionVersion)
        {
            Id = id;
            ProjectionVersion = projectionVersion;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
        }

        [DataMember(Order = 1)]
        public ProjectionArId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }

        [DataMember(Order = 3)]
        public long Timestamp { get; private set; }
    }

    [DataContract(Name = "9c0c789f-1605-460b-b179-cfdf2a697a1c")]
    public class ReplayProjectionFinished : IEvent
    {
        ReplayProjectionFinished() { }

        public ReplayProjectionFinished(ProjectionArId id, ProjectionVersion projectionVersion)
        {
            Id = id;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            ProjectionVersion = projectionVersion;
        }

        [DataMember(Order = 1)]
        public ProjectionArId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }

        [DataMember(Order = 3)]
        public long Timestamp { get; private set; }
    }

    [DataContract(Name = "e9731995-bfba-4de8-bd01-f5d8c06dba6d")]
    public class ReplayProjectionCanceled : IEvent
    {
        ReplayProjectionCanceled() { }

        public ReplayProjectionCanceled(ProjectionArId id, ProjectionVersion projectionVersion)
        {
            Id = id;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            ProjectionVersion = projectionVersion;
        }

        [DataMember(Order = 1)]
        public ProjectionArId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }

        [DataMember(Order = 3)]
        public long Timestamp { get; private set; }
    }

    public class ProjectionArId : StringId
    {
        ProjectionArId() : base() { }

        public ProjectionArId(string id) : base(id, "ProjectionAR") { }
    }


}
