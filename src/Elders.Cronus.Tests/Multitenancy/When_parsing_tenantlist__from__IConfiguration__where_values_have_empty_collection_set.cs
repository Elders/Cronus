using Machine.Specifications;
using System;

namespace Elders.Cronus.Multitenancy
{
    [Subject("Tenants")]
    public class When_parsing_tenantlist__from__IConfiguration__where_values_have_empty_collection_set
    {
        Establish context = () =>
        {
            configuration = new MockedConfiguration();
            configuration[Tenants.SettingKey] = @"[]";
        };

        Because of = () => exception = Catch.Exception(() => new Tenants(configuration));

        It should_throw_exception = () => exception.ShouldNotBeNull();

        It should_throw__ArgumentException__ = () => exception.ShouldBeOfExactType<ArgumentException>();

        It should_have_doc_in_exception_message = () => exception.Message.ShouldContain("Configuration.md");

        static Exception exception;
        static MockedConfiguration configuration;
    }
}
