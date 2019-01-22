using System;
using System.Collections.Generic;
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

            EnsureThereIsNoOutdatedBuildingVersions();
        }

        public void Replay(string hash) // maybe rename to Rebuild
        {
            EnsureThereIsNoOutdatedBuildingVersions();

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

            EnsureThereIsNoOutdatedBuildingVersions();
        }

        public void NotifyHash(string hash)
        {
            EnsureThereIsNoOutdatedBuildingVersions();

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

            EnsureThereIsNoOutdatedBuildingVersions();
        }

        private bool HasLiveVersion() => state.Versions.GetLive() is null == false;

        private bool HasBuildingVersion()
        {
            return state.Versions
                .Where(ver => ver.Status == ProjectionStatus.Building)
                .OrderByDescending(x => x.Revision)
                .Any();
        }

        private bool CanReplayHash(string hash)
        {
            bool isHashTheLiveOne = IsHashTheLiveOne(hash);

            return HasBuildingVersion() == false && (HasLiveVersion() == false || isHashTheLiveOne);
        }

        private void EnsureThereIsNoOutdatedBuildingVersions()
        {
            IEnumerable<ProjectionVersion> buildingVersions = state.Versions.WithoutTheGarbage()
                .Where(ver => ver.Status == ProjectionStatus.Building)
                .OrderByDescending(x => x.Revision);

            foreach (var buildingVersion in buildingVersions)
            {
                if (state.LastVersionRequestTimebox.HasExpired)
                    VersionRequestTimedout(buildingVersion, state.LastVersionRequestTimebox);
                else if (HasLiveVersion() && buildingVersion < state.Versions.GetLive())
                    CancelVersionRequest(buildingVersion, "Outdated version. There is already a live version.");
            }
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
        /// <returns>The timebox for the requested hash</returns>
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
