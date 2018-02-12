using System;

namespace Elders.Cronus.Projections.Versioning
{
    public static class ProjectionExtensions
    {
        static ProjectionHasher hasher = new ProjectionHasher();
        public static string GetProjectionHash(this Type projectionType)
        {
            return hasher.CalculateHash(projectionType);
        }
    }
}
