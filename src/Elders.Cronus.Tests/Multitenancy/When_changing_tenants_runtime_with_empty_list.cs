//using Machine.Specifications;
//using System;
//using System.Linq;

//namespace Elders.Cronus.Multitenancy
//{
//    [Subject("Tenants")]
//    public class When_changing_tenants_runtime_with_empty_list
//    {
//        Establish context = () =>
//        {
//            options = new TenantsOptionsMonitorMock("tenant1", "tenant2");
//            tenants = new Tenants(options);
//        };

//        Because of = () => exception = Catch.Exception(() => options.Change(new TenantsOptions { Tenants = new string[] { } }));

//        It should_throw_exception = () => exception.ShouldNotBeNull();

//        It should_throw__ArgumentException__ = () => exception.ShouldBeOfExactType<ArgumentException>();

//        It should_have_doc_in_exception_message = () => exception.Message.ShouldContain("Configuration.md");

//        It should_keep_the_old_tenants = () => tenants.GetTenants().Count().ShouldEqual(2);

//        It should_keep_the_first_tenant = () => tenants.GetTenants().First().ShouldEqual("tenant1");

//        It should_keep_the_second_tenant = () => tenants.GetTenants().Last().ShouldEqual("tenant2");

//        static Exception exception;
//        static TenantsOptionsMonitorMock options;
//        static Tenants tenants;
//    }
//}
