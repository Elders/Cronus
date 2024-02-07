using System;
using System.Collections.Generic;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Projections
{
    internal class ProjectionFinderViaReflection : IProjectionVersionFinder
    {
        private readonly TypeContainer<IProjection> allProjections;
        private readonly ProjectionHasher hasher;

        public ProjectionFinderViaReflection(TypeContainer<IProjection> allProjections, ProjectionHasher hasher)
        {
            this.allProjections = allProjections;
            this.hasher = hasher;
        }

        public IEnumerable<ProjectionVersion> GetProjectionVersionsToBootstrap()
        {
            foreach (Type projectionType in allProjections.Items)
            {
                if (typeof(IProjectionDefinition).IsAssignableFrom(projectionType) || typeof(IAmEventSourcedProjection).IsAssignableFrom(projectionType))
                {
                    string name = projectionType.GetContractId();
                    string hash = hasher.CalculateHash(projectionType);

                    yield return new ProjectionVersion(name, ProjectionStatus.NotPresent, 1, hash);
                }
            }
        }
    }
}
