using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.ProjectionStore
{

    public interface IProjectionStore
    {
        void Append(ProjectionCommit projectionCommit);
        ProjectionStream Load(IAggregateRootId aggregateId);
    }
}