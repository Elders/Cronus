using Elders.Cronus.DomainModeling;
using Elders.Cronus.InMemory;
using Elders.Cronus.IocContainer;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IInMemoryCommandPublisherSettings : ISettingsBuilder { }

    public class InMemoryCommandPublisherSettings : IInMemoryCommandPublisherSettings
    {
        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;
            builder.Container.RegisterSingleton<IPublisher<ICommand>>(() => new InMemoryCommandPublisher(builder.Container.Resolve<IMessageProcessor<ICommand>>(builder.Name)));
        }
    }

    public interface IInMemoryEventPublisherSettings : ISettingsBuilder { }

    public class InMemoryEventPublisherSettings : IInMemoryEventPublisherSettings
    {
        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;
            builder.Container.RegisterSingleton<IPublisher<IEvent>>(() => new InMemoryEventPublisher(builder.Container.Resolve<IMessageProcessor<IEvent>>(builder.Name)));
        }
    }
}