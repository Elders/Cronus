using System;

namespace Elders.Cronus.Projections
{
    public static class ProjectionExtensions
    {
        static Versioning.ProjectionHasher hasher = new Versioning.ProjectionHasher();
        public static string GetProjectionHash(this Type projectionType)
        {
            return hasher.CalculateHash(projectionType);
        }

        public static bool IsSnapshotable(this Type projectionType)
        {
            return typeof(Snapshotting.IAmNotSnapshotable).IsAssignableFrom(projectionType) == false;
        }
    }
}
