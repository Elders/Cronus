using System.Collections.Generic;

namespace Elders.Cronus.Multitenancy
{
    public class TenantsOptions
    {
        public IEnumerable<string> Tenants { get; set; }
    }
}
