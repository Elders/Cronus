using System.Collections.Generic;

namespace Elders.Cronus.EventStore
{
    public interface ITenantList
    {
        IEnumerable<string> GetTenants();
    }
}
