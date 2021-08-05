using System;

namespace Elders.Cronus.Projections
{
    public static class ProjectionExtensions
    {
        public static bool IsSnapshotable(this Type projectionType)
        {
            return typeof(Snapshotting.IAmNotSnapshotable).IsAssignableFrom(projectionType) == false;
        }
    }
}
