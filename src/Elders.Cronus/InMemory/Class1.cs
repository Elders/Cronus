using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.InMemory;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IInMemoryCommandPublisherSettings : IHaveMessageProcessor<ICommand>, ISettingsBuilder<IPublisher<ICommand>>
    {
    }

    public class InMemoryCommandPublisherSettings : IInMemoryCommandPublisherSettings
    {
        Lazy<IMessageProcessor<ICommand>> IHaveMessageProcessor<ICommand>.MessageHandlerProcessor { get; set; }

        Lazy<IPublisher<ICommand>> ISettingsBuilder<IPublisher<ICommand>>.Build()
        {
            IInMemoryCommandPublisherSettings settings = this as IInMemoryCommandPublisherSettings;
            return new Lazy<IPublisher<ICommand>>(() => new InMemoryCommandPublisher(settings.MessageHandlerProcessor.Value));
        }
    }

    public interface IInMemoryEventPublisherSettings : IHaveMessageProcessor<IEvent>, ISettingsBuilder<IPublisher<IEvent>>
    {
    }

    public class InMemoryEventPublisherSettings : IInMemoryEventPublisherSettings
    {
        Lazy<IMessageProcessor<IEvent>> IHaveMessageProcessor<IEvent>.MessageHandlerProcessor { get; set; }

        Lazy<IPublisher<IEvent>> ISettingsBuilder<IPublisher<IEvent>>.Build()
        {
            IInMemoryEventPublisherSettings settings = this as IInMemoryEventPublisherSettings;
            return new Lazy<IPublisher<IEvent>>(() => new InMemoryEventPublisher(settings.MessageHandlerProcessor.Value));
        }
    }

}
