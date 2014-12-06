using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.IocContainer
{

    [Subject("IocContainer")]
    public class When_resolve_scoped_component_with_specific_type
    {
        Establish context = () =>
            {
                container = new Container();
                container.RegisterScoped(typeof(BatchComponent), () => new BatchComponent(), ScopeType.PerBatch);
            };

        Because of = () =>
            {
                using (container.BeginScope(ScopeType.PerBatch))
                {
                    using (container.BeginScope())
                    {
                        first = container.Resolve<BatchComponent>();
                    }
                    using (container.BeginScope())
                    {
                        second = container.Resolve<BatchComponent>();
                    }
                }
            };

        It should_instantiate_both_objects = () => { first.ShouldNotBeNull(); second.ShouldNotBeNull(); };
        It should_be_the_same_object = () => first.ShouldBeTheSameAs(second);


        static IContainer container;
        static BatchComponent first;
        static BatchComponent second;
    }
}