using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.IocContainer
{
    [Subject("IocContainer")]
    public class When_asking_if_registered_on_something_that_is_not_registered
    {
        Establish context = () =>
        {
            container = new Container();

        };

        Because of = () => isRegistered = container.IsRegistered(typeof(BatchComponent));

        It should_be_registered = () => isRegistered.ShouldBeFalse();

        static IContainer container;
        static bool isRegistered;
    }
}