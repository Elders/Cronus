using Machine.Specifications;
using System.Linq;

namespace Elders.Cronus.Multitenancy
{
    [Subject("Projections")]
    public class When_parsing_tenantlist_from__IConfiguration__
    {
        Establish context = () =>
        {
            configuration = new ConfigurationMock();
            configuration[Tenants.SettingKey] = @"[""tenant1"", ""tenant2""]";
        };

        Because of = () => tenants = new Tenants(configuration);

        It should_be_able_to_get_all_tenants = () => tenants.GetTenants().Count().ShouldEqual(2);

        It should_be_able_to_get_first_tenant = () => tenants.GetTenants().First().ShouldEqual("tenant1");

        It should_be_able_to_get_second_tenant = () => tenants.GetTenants().Last().ShouldEqual("tenant2");

        static ConfigurationMock configuration;
        static Tenants tenants;
    }
}
