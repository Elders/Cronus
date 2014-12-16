namespace Elders.Cronus.ProjectionStore
{
    public sealed class ProjectionRepository
    {
        private readonly IProjectionStore eventStore;

        public ProjectionRepository(IProjectionStore eventStore)
        {
            this.eventStore = eventStore;
        }



    }

}