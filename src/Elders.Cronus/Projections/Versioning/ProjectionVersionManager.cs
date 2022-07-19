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
            var initialVersion = new ProjectionVersion(projectionName, ProjectionStatus.Replaying, 1, hash);
            var timebox = new VersionRequestTimebox(DateTime.UtcNow);
            RequestVersion(id, initialVersion, timebox);
        }

        public void CancelVersionRequest(ProjectionVersion version, string reason)
        {
            if (CanCancel(version))
            {
                if (version.MaybeIsBroken())
                {
                    foreach (ProjectionVersion buildingVersion in state.Versions.GetBuildingVersions())
                    {
                        ProjectionVersionRequestCanceled reset = new ProjectionVersionRequestCanceled(state.Id, buildingVersion.WithStatus(ProjectionStatus.Canceled), reason + " Something wrong has happened. We are trying to reset the state so you could try rebuild/replay the state.");
                        Apply(reset);
                    }
                }

                var @event = new ProjectionVersionRequestCanceled(state.Id, version.WithStatus(ProjectionStatus.Canceled), reason);
                Apply(@event);
            }
        }

        /// <summary>
        /// Replay all events into a new version of the projection.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="policy"></param>
        public void Replay(string hash, IProjectionVersioningPolicy policy)
        {
            EnsureThereIsNoOutdatedBuildingVersions();

            if (CanReplay(hash, policy))
            {
                ProjectionVersion projectionVersion = state.Versions.GetNext(policy, hash);
                VersionRequestTimebox timebox = GetVersionRequestTimebox(hash);
                RequestVersion(state.Id, projectionVersion, timebox);
            }
        }

        public void Rebuild(string hash)
        {
            EnsureThereIsNoOutdatedBuildingVersions();

            if (CanRebuild(hash))
            {
                ProjectionVersion currentLiveVersion = state.Versions.GetLive();
                var timebox = GetVersionRequestTimebox(hash);
                RequestVersion(state.Id, currentLiveVersion.WithStatus(ProjectionStatus.Rebuilding), timebox);
            }
        }

        public void VersionRequestTimedout(ProjectionVersion version, VersionRequestTimebox timebox)
        {
            // TODO: check if the timebox really has expired LOL :), Believe me, do it
            // Ask the SAGA if this is for real??
            bool foundVersion = state.Versions.Contains(version);
            if (foundVersion == false) return;

            if (version.Status == ProjectionStatus.Rebuilding || version.Status == ProjectionStatus.Replaying || version.Status == ProjectionStatus.Building)
            {
                var @event = new ProjectionVersionRequestTimedout(state.Id, version.WithStatus(ProjectionStatus.Timedout), timebox);
                Apply(@event);
            }
        }

        public void NotifyHash(string hash, IProjectionVersioningPolicy policy)
        {
            EnsureThereIsNoOutdatedBuildingVersions();

            if (ShouldReplay(hash))
            {
                Replay(hash, policy);
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

        private bool ShouldReplay(string hash)
        {
            bool isNewHashTheLiveOne = state.Versions.IsHashTheLiveOne(hash);
            bool isInProgress = state.Versions.HasBuildingVersion();

            return isInProgress == false && (state.Versions.HasLiveVersion == false || isNewHashTheLiveOne == false);
        }

        private bool CanReplay(string hash, IProjectionVersioningPolicy policy)
        {
            bool isNewHashTheLiveOne = state.Versions.IsHashTheLiveOne(hash);

            bool isVersionable = state.Versions.IsVersionable(policy);
            bool doesntHaveBuildingVersion = state.Versions.HasBuildingVersion() == false;

            return (doesntHaveBuildingVersion && isVersionable) || (doesntHaveBuildingVersion && isVersionable == false && isNewHashTheLiveOne == false);
        }

        private bool CanRebuild(string hash)
        {
            ProjectionVersion currentLiveVersion = state.Versions.GetLive();
            if (currentLiveVersion is null)
                return true;

            bool hashMatchesCurrentLiveVersion = currentLiveVersion.Hash.Equals(hash);
            bool hasRebuildingVersion = state.Versions.HasRebuildingVersion();

            return hashMatchesCurrentLiveVersion && hasRebuildingVersion == false;
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
            if (version.MaybeIsBroken())
                return true;

            if (version.Status != ProjectionStatus.Replaying && version.Status != ProjectionStatus.Rebuilding)
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
