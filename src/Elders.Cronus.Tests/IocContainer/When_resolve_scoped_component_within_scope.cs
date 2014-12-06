using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.IocContainer
{
    [Subject("IocContainer")]
    public class When_resolve_scoped_component_within_scope
    {
        Establish context = () =>
            {
                container = new Container();
                container.RegisterScoped(typeof(BatchComponent), () => new BatchComponent());
            };

        Because of = () =>
            {
                using (container.BeginScope())
                {
                    batchComponent = container.Resolve<BatchComponent>();
                }
            };

        It should_not_be_null = () => batchComponent.ShouldNotBeNull();
        It should_be_of_correctType = () => batchComponent.ShouldBeOfExactType<BatchComponent>();

        static IContainer container;
        static BatchComponent batchComponent;
    }
}