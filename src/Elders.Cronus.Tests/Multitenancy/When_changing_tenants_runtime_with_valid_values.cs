using Machine.Specifications;
using System.Linq;

namespace Elders.Cronus.Multitenancy
{
    [Subject("Tenants")]
    public class When_changing_tenants_runtime_with_valid_values
    {
        Establish context = () =>
        {
            options = new TenantsOptionsMonitorMock("tenant1", "tenant2");
            tenants = new Tenants(options);
        };

        Because of = () => options.Change(new TenantsOptions { Tenants = new string[] { "tenant2", "tenant3" } });

        It should_be_able_to_get_all_tenants = () => tenants.GetTenants().Count().ShouldEqual(2);

        It should_be_able_to_get_first_tenant = () => tenants.GetTenants().First().ShouldEqual("tenant2");

        It should_be_able_to_get_second_tenant = () => tenants.GetTenants().Last().ShouldEqual("tenant3");

        It should_not_contain_the_old_tenant = () => tenants.GetTenants().ShouldNotContain("tenant1");

        static TenantsOptionsMonitorMock options;
        static Tenants tenants;
    }
}
