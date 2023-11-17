using Elders.Cronus.EventStore.Index.Handlers;
using Elders.Cronus.Projections.Versioning;
using System;

namespace Elders.Cronus.Projections
{
    public static class ProjectionExtensions
    {
        public static bool IsProjectionVersionHandler(this string projectionName)
        {
            return projectionName.Equals(ProjectionVersionsHandler.ContractId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsEventStoreIndexStatus(this string projectionName)
        {
            return projectionName.Equals(EventStoreIndexStatus.ContractId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
