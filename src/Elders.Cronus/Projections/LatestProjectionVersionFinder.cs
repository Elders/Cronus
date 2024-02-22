using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Projections;

public class LatestProjectionVersionFinder
{
    private readonly IEnumerable<IProjectionVersionFinder> projectionFinders;

    public LatestProjectionVersionFinder(IEnumerable<IProjectionVersionFinder> projectionFinders)
    {
        this.projectionFinders = projectionFinders;
    }

    public IEnumerable<ProjectionVersion> GetProjectionVersionsToBootstrap()
    {
        var allPossibleVersions = projectionFinders.SelectMany(finder => finder.GetProjectionVersionsToBootstrap()).GroupBy(ver => ver.ProjectionName);

        foreach (var versionGroup in allPossibleVersions)
        {
            ProjectionVersion lastLiveVersion = versionGroup.Where(ver => ver.Status == ProjectionStatus.Live).MaxBy(ver => ver.Revision);
            if (lastLiveVersion is not null)
                yield return lastLiveVersion;
            else
                yield return versionGroup.First();
        }
    }
}
