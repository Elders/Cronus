using System.Collections.Generic;

namespace Elders.Cronus.Projections;

public interface IProjectionVersionFinder
{
    IEnumerable<ProjectionVersion> GetProjectionVersionsToBootstrap();
}
