using System.Collections.Generic;

namespace Elders.Cronus.Multitenancy
{
    public interface ITenantList : ICronusService
    {
        IEnumerable<string> GetTenants();
    }
}
