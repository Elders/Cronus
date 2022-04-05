using Elders.Cronus.Projections.Versioning;
using System;

namespace Elders.Cronus.Projections
{
    public static class ProjectionExtensions
    {
        public static bool IsSnapshotable(this Type projectionType)
        {
            return typeof(Snapshotting.IAmNotSnapshotable).IsAssignableFrom(projectionType) == false;
        }

        public static bool IsProjectionVersionHandler(this string projectionName)
        {
            return projectionName.Equals(ProjectionVersionsHandler.ContractId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
