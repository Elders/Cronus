using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.IocContainer
{
    [Subject("IocContainer")]
    public class When_resolve_scoped_component_within_two_not_nested_scopes
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
                    first = container.Resolve<BatchComponent>();
                }

                using (container.BeginScope())
                {
                    second = container.Resolve<BatchComponent>();
                }
            };

        It should_instantiate_both_objects = () => { first.ShouldNotBeNull(); second.ShouldNotBeNull(); };
        It should_not_be_the_same_object = () => first.ShouldNotBeTheSameAs(second);


        static IContainer container;
        static BatchComponent first;
        static BatchComponent second;
    }
}