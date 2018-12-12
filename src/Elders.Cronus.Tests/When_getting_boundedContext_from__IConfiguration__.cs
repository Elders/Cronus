using Machine.Specifications;
using System;

namespace Elders.Cronus
{
    [Subject("BoundedContext")]
    public class When_getting_invalid_boundedContext_from__IConfiguration__
    {
        Establish context = () =>
        {
            configuration = new ConfigurationMock();
            configuration[BoundedContext.SettingKey] = @"elders.test";
        };

        Because of = () => exception = Catch.Exception(() => new BoundedContext(configuration));

        It should_throw_exception = () => exception.ShouldNotBeNull();

        It should_throw__ArgumentException__ = () => exception.ShouldBeOfExactType<ArgumentException>();

        static Exception exception;
        static ConfigurationMock configuration;
    }
}
