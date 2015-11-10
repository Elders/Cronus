using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.IocContainer
{
    [Subject("IocContainer")]
    public class When_resolve_two_scoped_components_with_common_interface
    {
        Establish context = () =>
            {
                container = new Container();
                container.RegisterScoped<ITestUntOfWork, PerMessageUnitOfWork>(ScopeType.PerMessage);
                container.RegisterScoped<ITestUntOfWork, PerHandlerUnitOfWork>(ScopeType.PerHandler);
            };

        Because of = () =>
        {
            using (container.BeginScope(ScopeType.PerMessage))
            {
                perMessage = container.Resolve<ITestUntOfWork>();
                using (container.BeginScope(ScopeType.PerHandler))
                {
                    perHandler = container.Resolve<ITestUntOfWork>();
                }
            }
        };

        It should_create_separate_instances_depending_on_the_scope_type = () =>
            {
                perMessage.ShouldBeOfExactType<PerMessageUnitOfWork>();
                perHandler.ShouldBeOfExactType<PerHandlerUnitOfWork>();
            };

        static IContainer container;
        static ITestUntOfWork perMessage;
        static ITestUntOfWork perHandler;
    }

    [Subject("IocContainer")]
    public class When_resolve_two_scoped_components_with_common_aaaaaaaaaaa
    {
        Establish context = () =>
            {
                container = new Container();

                container.RegisterScoped<ITestUntOfWork, PerMessageUnitOfWork>(ScopeType.PerMessage);
                container.RegisterScoped<ITestUntOfWork, PerMessageUnitOfWork>(ScopeType.PerHandler);
                container.RegisterScoped<BatchComponent>(() => new BatchComponent("shared"));
            };

        Because of = () =>
        {
            using (container.BeginScope(ScopeType.PerMessage))
            {
                perMessage = container.Resolve<ITestUntOfWork>();
                shared = container.Resolve<BatchComponent>();
                shared.Name = "perMessage";
                using (container.BeginScope(ScopeType.PerHandler))
                {
                    perHandler = container.Resolve<ITestUntOfWork>();
                    shared = container.Resolve<BatchComponent>();
                }
            }
        };

        It should_create_separate_instances_depending_on_the_scope_type = () =>
            {
                perMessage.ShouldBeOfExactType<PerMessageUnitOfWork>();
                perHandler.ShouldBeOfExactType<PerMessageUnitOfWork>();
            };

        It should_create_different_objects = () =>
            {
                perMessage.ShouldNotBeTheSameAs(perHandler);
            };

        static IContainer container;
        static ITestUntOfWork perMessage;
        static ITestUntOfWork perHandler;
        static BatchComponent shared;
    }
}