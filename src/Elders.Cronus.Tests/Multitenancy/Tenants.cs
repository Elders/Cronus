using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elders.Cronus.Multitenancy
{

    [Subject("Projections")]
    public class When_parsing_tenantlist_from__IConfiguration__
    {
        Establish context = () =>
        {
            configuration = new MockedConfiguration();
            configuration["cronus_tenants"] = @"[""tenant1"", ""tenant2""]";
        };

        Because of = () => tenants = new Tenants(configuration);

        It should_be_able_to_get_all_tenants = () => tenants.GetTenants().Count().ShouldEqual(2);

        It should_be_able_to_get_first_tenant = () => tenants.GetTenants().First().ShouldEqual("tenant1");

        It should_be_able_to_get_second_tenant = () => tenants.GetTenants().Last().ShouldEqual("tenant2");

        static MockedConfiguration configuration;
        static Tenants tenants;
    }

    [Subject("Tenants")]
    public class When_parsing_tenantlist__from__IConfiguration__where_values_have_spaces
    {
        Establish context = () =>
        {
            configuration = new MockedConfiguration();
            configuration["cronus_tenants"] = @"["" tenant1 "", "" tenant2 ""]";
        };

        Because of = () => tenants = new Tenants(configuration);

        It should_be_able_to_get_all_tenants = () => tenants.GetTenants().Count().ShouldEqual(2);

        It should_be_able_to_get_first_tenant = () => tenants.GetTenants().First().ShouldEqual("tenant1");

        It should_be_able_to_get_second_tenant = () => tenants.GetTenants().Last().ShouldEqual("tenant2");

        static MockedConfiguration configuration;
        static Tenants tenants;
    }

    [Subject("Tenants")]
    public class When_parsing_tenantlist__from__IConfiguration__where_values_have_mixed_casing
    {
        Establish context = () =>
        {
            configuration = new MockedConfiguration();
            configuration["cronus_tenants"] = @"["" tEnant1 "", ""tEnant2""]";
        };

        Because of = () => tenants = new Tenants(configuration);

        It should_be_able_to_get_all_tenants = () => tenants.GetTenants().Count().ShouldEqual(2);

        It should_be_able_to_get_first_tenant = () => tenants.GetTenants().First().ShouldEqual("tenant1");

        It should_be_able_to_get_second_tenant = () => tenants.GetTenants().Last().ShouldEqual("tenant2");

        static MockedConfiguration configuration;
        static Tenants tenants;
    }

    [Subject("Tenants")]
    public class When_parsing_tenantlist__from__IConfiguration__where_values_are_not_strict_json_array
    {
        Establish context = () =>
        {
            configuration = new MockedConfiguration();
            configuration["cronus_tenants"] = @"tEnant1 , tEnant2";
        };

        Because of = () => tenants = new Tenants(configuration);

        It should_be_able_to_get_all_tenants = () => tenants.GetTenants().Count().ShouldEqual(2);

        It should_be_able_to_get_first_tenant = () => tenants.GetTenants().First().ShouldEqual("tenant1");

        It should_be_able_to_get_second_tenant = () => tenants.GetTenants().Last().ShouldEqual("tenant2");

        static MockedConfiguration configuration;
        static Tenants tenants;
    }

    [Subject("Tenants")]
    public class When_parsing_tenantlist__from__IConfiguration__where_values_have_empty_collection_set
    {
        Establish context = () =>
        {
            configuration = new MockedConfiguration();
            configuration["cronus_tenants"] = @"[]";
        };

        Because of = () => exception = Catch.Exception(() => new Tenants(configuration));

        It should_throw_exception = () => exception.ShouldNotBeNull();

        It should_throw__ArgumentException__ = () => exception.ShouldBeOfExactType<ArgumentException>();

        It should_have_doc_in_exception_message = () => exception.Message.ShouldContain("Configuration.md");

        static Exception exception;
        static MockedConfiguration configuration;
        static Tenants tenants;
    }

    [Subject("Tenants")]
    public class When_parsing_tenantlist__from__IConfiguration__where_values_have_invalid_collection_set
    {
        Establish context = () =>
        {
            configuration = new MockedConfiguration();
            configuration["cronus_tenants"] = @"Market@_!Vision";
        };

        Because of = () => exception = Catch.Exception(() => new Tenants(configuration));

        It should_throw_exception = () => exception.ShouldNotBeNull();

        It should_throw__ArgumentException__ = () => exception.ShouldBeOfExactType<ArgumentException>();

        It should_have_doc_in_exception_message = () => exception.Message.ShouldContain("Configuration.md");

        static Exception exception;
        static MockedConfiguration configuration;
        static Tenants tenants;
    }
}
