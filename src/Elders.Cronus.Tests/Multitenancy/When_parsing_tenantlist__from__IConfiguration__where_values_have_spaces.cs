using Machine.Specifications;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Elders.Cronus.Multitenancy
{
    [Subject("Tenants")]
    public class When_parsing_tenantlist__from__IConfiguration__where_values_have_spaces
    {
        Establish context = () =>
        {
            options = new TenantsOptionsMonitorMock(" tenant1 ", " tenant2 ");
        };

        Because of = () => tenants = new Tenants(options);

        It should_be_able_to_get_all_tenants = () => tenants.GetTenants().Count().ShouldEqual(2);

        It should_be_able_to_get_first_tenant = () => tenants.GetTenants().First().ShouldEqual("tenant1");

        It should_be_able_to_get_second_tenant = () => tenants.GetTenants().Last().ShouldEqual("tenant2");

        static IOptionsMonitor<TenantsOptions> options;
        static Tenants tenants;
    }
}
