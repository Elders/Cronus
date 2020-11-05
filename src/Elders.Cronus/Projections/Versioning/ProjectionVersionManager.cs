using System;
using System.Collections.Generic;

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

        public void Rebuild(string hash, IProjectionVersioningPolicy policy)
        {
            EnsureThereIsNoOutdatedBuildingVersions();

            if (CanRebuild())
            {
                var projectionVersion = state.Versions.GetNext(policy, hash);
                var timebox = GetVersionRequestTimebox(hash);
                RequestVersion(state.Id, projectionVersion, timebox);
            }
        }

        public void VersionRequestTimedout(ProjectionVersion version, VersionRequestTimebox timebox)
        {
            // TODO: check if the timebox really has expired LOL :), Believe me, do it
            // Ask the SAGA if this is for real??
            bool foundVersion = state.Versions.Contains(version);
            if (foundVersion == false) return;

            if (version.Status == ProjectionStatus.Building)
            {
                var @event = new ProjectionVersionRequestTimedout(state.Id, version.WithStatus(ProjectionStatus.Timedout), timebox);
                Apply(@event);
            }

            EnsureThereIsNoOutdatedBuildingVersions();
        }

        public void NotifyHash(string hash, IProjectionVersioningPolicy policy)
        {
            EnsureThereIsNoOutdatedBuildingVersions();

            if (ShouldRebuild(hash))
            {
                Rebuild(hash, policy);
            }
        }

        public void FinalizeVersionRequest(ProjectionVersion version)
        {
            var isVersionFound = state.Versions.Contains(version);
            if (isVersionFound)
            {
                var @event = new NewProjectionVersionIsNowLive(state.Id, version.WithStatus(ProjectionStatus.Live));
                Apply(@event);
            }

            EnsureThereIsNoOutdatedBuildingVersions();
        }

        private bool ShouldRebuild(string hash)
        {
            bool isNewHashTheLiveOne = state.Versions.IsHashTheLiveOne(hash);

            return state.Versions.HasLiveVersion == false || isNewHashTheLiveOne == false;
        }

        private bool CanRebuild()
        {
            bool doesntHaveBuildingVersion = state.Versions.HasBuildingVersion() == false;
            return doesntHaveBuildingVersion;
        }

        private void EnsureThereIsNoOutdatedBuildingVersions()
        {
            IEnumerable<ProjectionVersion> buildingVersions = state.Versions.GetBuildingVersions();

            foreach (var buildingVersion in buildingVersions)
            {
                if (state.LastVersionRequestTimebox.HasExpired)
                    VersionRequestTimedout(buildingVersion, state.LastVersionRequestTimebox);

                if (state.Versions.HasLiveVersion && buildingVersion < state.Versions.GetLive())
                    CancelVersionRequest(buildingVersion, "Outdated version. There is already a live version.");
            }
        }

        private bool CanCancel(ProjectionVersion version)
        {
            if (version.Status != ProjectionStatus.Building)
                return false;

            return state.Versions.Contains(version);
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
