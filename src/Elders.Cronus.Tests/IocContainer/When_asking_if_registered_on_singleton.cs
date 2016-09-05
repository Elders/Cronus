using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.IocContainer
{

    [Subject("IocContainer")]
    public class When_asking_if_registered_on_singleton
    {
        Establish context = () =>
        {
            container = new Container();
            container.RegisterSingleton(typeof(BatchComponent), () => new BatchComponent());
        };

        Because of = () => isRegistered = container.IsRegistered(typeof(BatchComponent));

        It should_be_registered = () => isRegistered.ShouldBeTrue();

        static IContainer container;
        static bool isRegistered;
    }
}