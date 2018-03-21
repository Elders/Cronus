using System.Collections.Generic;

namespace Elders.Cronus.Multitenancy
{
    public interface ITenantList
    {
        IEnumerable<string> GetTenants();
    }
}
