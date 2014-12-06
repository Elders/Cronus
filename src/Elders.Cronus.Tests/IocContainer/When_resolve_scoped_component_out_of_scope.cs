using System;
using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.IocContainer
{

    [Subject("IocContainer")]
    public class When_resolve_scoped_component_out_of_scope
    {
        Establish context = () =>
            {
                container = new Container();
                container.RegisterScoped(typeof(BatchComponent), () => new BatchComponent());
            };

        Because of = () => expectedException = Catch.Exception(() => container.Resolve<BatchComponent>());

        It an__InvalidOperationException__should_be_thrown = () => expectedException.ShouldBeOfExactType<InvalidOperationException>();

        static IContainer container;
        static Exception expectedException;
    }
}