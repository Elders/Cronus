using System;
using System.Linq;

namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionVersionManager : AggregateRoot<ProjectionVersionManagerState>
    {
        ProjectionVersionManager() { }

        public ProjectionVersionManager(ProjectionVersionManagerId id, string hash)
        {
            string projectionName = id.Id;
            var initialVersion = new ProjectionVersion(projectionName, ProjectionStatus.Building, 1, hash);
            var timebox = new VersionRequestTimebox(DateTime.UtcNow);
            RequestVersion(id, initialVersion, timebox);
        }

        public void CancelVersionRequest(ProjectionVersion version, string reason)
        {
            if (CanCancel(version))
            {
                var @event = new ProjectionVersionRequestCanceled(state.Id, version.WithStatus(ProjectionStatus.Canceled), reason);
                Apply(@event);
            }
        }

        public void Replay(string hash) // maybe rename to Rebuild
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
            var foundVersion = state.Versions.Where(ver => ver == version).SingleOrDefault();
            if (ReferenceEquals(null, foundVersion)) return; // Should we do something about this? It is a not expected and should never happen!!!

            if (foundVersion.Status == ProjectionStatus.Building)
            {
                var @event = new ProjectionVersionRequestTimedout(state.Id, version.WithStatus(ProjectionStatus.Timedout), timebox);
                Apply(@event);
            }
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
            bool hasLiveVersion = ReferenceEquals(null, state.Versions.GetLive()) == false;
            ProjectionVersion buildingVersion = state.Versions
                .Where(ver => ver.Status == ProjectionStatus.Building && ver.Hash.Equals(hash, StringComparison.Ordinal))
                .OrderByDescending(x => x.Revision)
                .FirstOrDefault();
            bool hasBuildingVersion = ReferenceEquals(null, buildingVersion) == false;
            bool hasOutdatedBuildingVersion = hasLiveVersion && hasBuildingVersion && buildingVersion < state.Versions.GetLive() && isHashUsedBefore == false;

            return hasOutdatedBuildingVersion == false && hasBuildingVersion == false && (hasLiveVersion == false || isHashUsedBefore == false || isHashTheLiveOne);
        }

        private bool IsHashTheLiveOne(string hash)
        {
            if (HasLiveVersion() == false)
                return false;

            bool isHashTheLiveOne = state.Versions.GetLive().Hash.Equals(hash, StringComparison.Ordinal);
            return isHashTheLiveOne;
        }

        private bool CanCancel(ProjectionVersion version)
        {
            if (version.Status != ProjectionStatus.Building)
                return false;

            return state.Versions.Where(x => x == version).Any();
        }

        /// <summary>
        /// When the same hash is requested multiple times for a Live hash we just make sure that the timeboxes are placed one after another
        /// In every other case we issue a timebox with immediate execution
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
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
