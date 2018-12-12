using Machine.Specifications;

namespace Elders.Cronus
{
    [Subject("BoundedContext")]
    public class When_getting_valid_boundedContext_from__IConfiguration__
    {
        Establish context = () =>
        {
            configuration = new ConfigurationMock();
            configuration[BoundedContext.SettingKey] = "elders_test";
        };

        Because of = () => boundedContext = new BoundedContext(configuration);

        It should_have_correct_name = () => boundedContext.Name.ShouldBeTheSameAs("elders_test");

        static BoundedContext boundedContext;
        static ConfigurationMock configuration;
    }
}
