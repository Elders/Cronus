using System.Collections.Generic;

namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionVersionManagerState : AggregateRootState<ProjectionVersionManager, ProjectionVersionManagerId>
    {
        public ProjectionVersionManagerState()
        {
            Versions = new ProjectionVersions();
            HashHistoryOfLiveVersions = new HashSet<string>();
        }

        public override ProjectionVersionManagerId Id { get; set; }

        public ProjectionVersions Versions { get; set; }

        public HashSet<string> HashHistoryOfLiveVersions { get; set; }

        public VersionRequestTimebox LastVersionRequestTimebox { get; set; }

        public void When(ProjectionVersionRequestedForReplay e)
        {
            Id = e.Id;
            Versions.Add(e.Version);
            LastVersionRequestTimebox = e.Timebox;
        }

        public void When(NewProjectionVersionIsNowLive e)
        {
            Id = e.Id;
            Versions.Add(e.ProjectionVersion);
            LastVersionRequestTimebox = LastVersionRequestTimebox.Reset();
            HashHistoryOfLiveVersions.Add(e.ProjectionVersion.Hash);
        }

        public void When(ProjectionVersionRequestCanceled e)
        {
            Id = e.Id;
            Versions.Add(e.Version);
            LastVersionRequestTimebox = LastVersionRequestTimebox.Reset();
        }

        public void When(ProjectionVersionRequestTimedout e)
        {
            Id = e.Id;
            Versions.Add(e.Version);
            LastVersionRequestTimebox = LastVersionRequestTimebox.Reset();
        }

        public void When(ProjectionVersionRequestedForRebuild e)
        {
            Id = e.Id;
        }

        public void When(ProjectionVersionRebuildCanceled e)
        {
            Id = e.Id;
            Versions.Add(e.ProjectionVersion);
        }

        public void When(ProjectionFinishedRebuilding e)
        {
            Id = e.Id;
            Versions.Add(e.ProjectionVersion);
        }

        public void When(ProjectionVersionRebuildHasTimedout e)
        {
            Id = e.Id;
            Versions.Add(e.Version);
        }
    }
}
