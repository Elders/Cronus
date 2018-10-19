using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus
{
    public class ProjectionsBootstrapper
    {
        private readonly TypeContainer<IProjection> projectionTypes;
        private readonly ProjectionHasher hasher;
        private readonly IPublisher<ICommand> publisher;

        public ProjectionsBootstrapper(TypeContainer<IProjection> projectionTypes, ProjectionHasher hasher, IPublisher<ICommand> publisher)
        {
            this.projectionTypes = projectionTypes;
            this.hasher = hasher;
            this.publisher = publisher;
        }

        public void Bootstrap()
        {
            foreach (var handler in projectionTypes.Items)
            {
                var id = new ProjectionVersionManagerId(handler.GetContractId());
                var command = new RegisterProjection(id, hasher.CalculateHash(handler).ToString());
                publisher.Publish(command);
            }
        }
    }
}
