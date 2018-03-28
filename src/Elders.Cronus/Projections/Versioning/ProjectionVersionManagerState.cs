namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionVersionManagerState : AggregateRootState<ProjectionVersionManager, ProjectionVersionManagerId>
    {
        public ProjectionVersionManagerState()
        {
            Versions = new ProjectionVersions();
        }

        public override ProjectionVersionManagerId Id { get; set; }

        public ProjectionVersions Versions { get; set; }

        public VersionRequestTimebox LastVersionRequestTimebox { get; set; }

        public void When(ProjectionVersionRequestCanceled e)
        {
            Id = e.Id;
            Versions.Add(e.ProjectionVersion);
        }

        public void When(ProjectionVersionRequested e)
        {
            Id = e.Id;
            Versions.Add(e.ProjectionVersion);
            LastVersionRequestTimebox = e.Timebox;
        }

        public void When(NewProjectionVersionIsNowLive e)
        {
            Id = e.Id;
            Versions.Add(e.ProjectionVersion);
            LastVersionRequestTimebox = LastVersionRequestTimebox.Reset();
        }

        public void When(ProjectionVersionRequestTimedout e)
        {
            Id = e.Id;
            Versions.Add(e.Version);
            LastVersionRequestTimebox = LastVersionRequestTimebox.Reset();
        }
    }
}
