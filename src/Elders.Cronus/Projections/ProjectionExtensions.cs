using System;

namespace Elders.Cronus.Projections
{
    public static class ProjectionExtensions
    {
        public static bool IsSnapshotable(this Type projectionType)
        {
            return typeof(Snapshotting.IAmNotSnapshotable).IsAssignableFrom(projectionType) == false;
        }

        public static bool IsRebuildable(this ProjectionVersion version)
        {
            Type projectionType = version.ProjectionName.GetTypeByContract();
            bool isRebuildable = typeof(INonRebuildableProjection).IsAssignableFrom(projectionType) == false;

            return isRebuildable;
        }
    }
}
