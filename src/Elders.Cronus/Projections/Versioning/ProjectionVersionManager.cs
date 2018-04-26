using System;
using System.Linq;

namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionVersionManager : AggregateRoot<ProjectionVersionManagerState>
    {
        ProjectionVersionManager() { }

        public ProjectionVersionManager(ProjectionVersionManagerId id, string hash)
        {
            var initialVersion = new ProjectionVersion(id.Id, ProjectionStatus.Building, 1, hash);
            var timebox = new VersionRequestTimebox(DateTime.UtcNow);
            RequestVersion(id, initialVersion, timebox);
        }

        public void CancelVersionRequest(ProjectionVersion version)
        {
            if (CanCancel(version))
            {
                var projectionVersion = state.Versions.Where(x => x.Status == ProjectionStatus.Building).Single();
                var @event = new ProjectionVersionRequestCanceled(state.Id, projectionVersion.WithStatus(ProjectionStatus.Canceled));
                Apply(@event);
            }
        }

        public void Replay(string hash)
        {
            if (CanReplayHash(hash))
            {
                var projectionVersion = state.Versions.GetNext();
                var timebox = GetVersionRequestTimebox(hash);
                RequestVersion(state.Id, projectionVersion, timebox);
            }
        }

        public void VersionRequestTimedout(ProjectionVersion version, VersionRequestTimebox timebox)
        {
            var @event = new ProjectionVersionRequestTimedout(state.Id, version.WithStatus(ProjectionStatus.Timedout), timebox);
            Apply(@event);
        }

        public void NotifyHash(string hash)
        {
            if (HasLiveVersion() == false || IsHashTheLiveOne(hash) == false)
            {
                Replay(hash);
            }
        }

        public void FinalizeVersionRequest(ProjectionVersion version)
        {
            var buildingVersion = state.Versions.Where(x => x == version).SingleOrDefault();
            if (ReferenceEquals(null, buildingVersion) == false)
            {
                var @event = new NewProjectionVersionIsNowLive(state.Id, buildingVersion.WithStatus(ProjectionStatus.Live));
                Apply(@event);
            }
        }

        private bool HasLiveVersion()
        {
            ProjectionVersion liveVersion = state.Versions.GetLive();

            return ReferenceEquals(null, liveVersion) == false;
        }

        private bool CanReplayHash(string hash)
        {
            bool isHashUsedBefore = state.HashHistoryOfLiveVersions.Contains(hash);
            bool isHashTheLiveOne = IsHashTheLiveOne(hash);

            return isHashUsedBefore == false || isHashTheLiveOne;
        }

        private bool IsHashTheLiveOne(string hash)
        {
            bool isHashTheLiveOne = state.Versions.GetLive().Hash.Equals(hash, StringComparison.Ordinal);
            return isHashTheLiveOne;
        }

        private bool CanCancel(ProjectionVersion version)
        {
            if (version.Status != ProjectionStatus.Building)
                return false;

            return state.Versions.Any(x => x == version);
        }

        private VersionRequestTimebox GetVersionRequestTimebox(string hash)
        {
            ProjectionVersion live = state.Versions.GetLive();
            if (live == null) return new VersionRequestTimebox(DateTime.UtcNow);

            var hashesAreIdentical = string.Equals(live.Hash, hash, StringComparison.OrdinalIgnoreCase);
            if (hashesAreIdentical)
                return state.LastVersionRequestTimebox.GetNext();

            return new VersionRequestTimebox(DateTime.UtcNow);
        }

        private void RequestVersion(ProjectionVersionManagerId id, ProjectionVersion projectionVersion, VersionRequestTimebox timebox)
        {
            var @event = new ProjectionVersionRequested(id, projectionVersion, timebox);
            Apply(@event);
        }
    }
}
