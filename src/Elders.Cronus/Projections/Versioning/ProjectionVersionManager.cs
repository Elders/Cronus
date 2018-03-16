using System;
using System.Linq;

namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionVersionManager : AggregateRoot<ProjectionVersionManagerState>
    {
        ProjectionVersionManager() { }

        public ProjectionVersionManager(ProjectionVersionManagerId id, string hash)
        {
            ProjectionVersion initialVersion = new ProjectionVersion(id.Id, ProjectionStatus.Building, 1, hash);

            var timebox = new VersionRequestTimebox(DateTime.UtcNow);
            RequestVersion(id, initialVersion, timebox);
        }

        public void Replay(string hash)
        {
            var projectionVersion = new ProjectionVersion(state.Id.Id, ProjectionStatus.Building, 1, hash);
            var timebox = GetVersionRequestTimebox(hash);
            RequestVersion(state.Id, projectionVersion, timebox);
        }

        void RequestVersion(ProjectionVersionManagerId id, ProjectionVersion projectionVersion, VersionRequestTimebox timebox)
        {
            var @event = new ProjectionVersionRequested(id, projectionVersion, timebox);
            Apply(@event);
        }

        public void VersionRequestTimedout(ProjectionVersion version, VersionRequestTimebox timebox)
        {
            var @event = new ProjectionVersionRequestTimedout(state.Id, version.WithStatus(ProjectionStatus.Timedout), timebox);
            Apply(@event);
        }

        public void CancelVersionRequest()
        {
            if (CanCancel())
            {
                var projectionVersion = state.Versions.Where(x => x.Status == ProjectionStatus.Building).Single();
                var @event = new ProjectionVersionRequestCanceled(state.Id, projectionVersion.WithStatus(ProjectionStatus.Canceled));
                Apply(@event);
            }
        }

        bool CanCancel()
        {
            return state.Versions.Any(x => x.Status == ProjectionStatus.Building);
        }

        VersionRequestTimebox GetVersionRequestTimebox(string hash)
        {
            ProjectionVersion live = state.Versions.GetLive();
            if (live == null) return new VersionRequestTimebox(DateTime.UtcNow);

            var hashesAreIdentical = string.Equals(live.Hash, hash, StringComparison.OrdinalIgnoreCase);
            if (hashesAreIdentical)
                return state.LastVersionRequestTimebox.GetNext();

            return new VersionRequestTimebox(DateTime.UtcNow);
        }

        public void NotifyHash(string hash)
        {
            ProjectionVersion live = state.Versions.GetLive();

            if (live == null || string.Equals(live.Hash, hash, StringComparison.OrdinalIgnoreCase) == false)
            {
                Replay(hash);
            }
        }

        public void FinalizeVersionRequest(ProjectionVersion version)
        {
            var buildingVersion = state.Versions.Where(x => x == version).SingleOrDefault();
            // if (ReferenceEquals(null, buildingVersion) == false)
            {
                var @event = new NewProjectionVersionIsNowLive(state.Id, buildingVersion.WithStatus(ProjectionStatus.Live));
                Apply(@event);
            }
        }
    }
}
