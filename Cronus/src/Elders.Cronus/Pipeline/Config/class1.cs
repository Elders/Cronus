using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.InMemory;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IInMemoryCommandPublisherSettings : IConsumerSettings, IHaveMessageProcessor<ICommand>, ISettingsBuilder<IPublisher<ICommand>>
    {
    }

    public class InMemoryCommandPublisherSettings : IInMemoryCommandPublisherSettings
    {
        string IConsumerSettings.BoundedContext { get; set; }

        Lazy<IEndpointPostConsume> IHaveEndpointPostConsumeActions.PostConsume { get; set; }

        Serializer.ISerializer IHaveSerializer.Serializer { get; set; }

        Lazy<IMessageProcessor<ICommand>> IHaveMessageProcessor<ICommand>.MessageHandlerProcessor { get; set; }

        Lazy<IPublisher<ICommand>> ISettingsBuilder<IPublisher<ICommand>>.Build()
        {
            IInMemoryCommandPublisherSettings settings = this as IInMemoryCommandPublisherSettings;
            return new Lazy<IPublisher<ICommand>>(() => new InMemoryCommandPublisher(settings.MessageHandlerProcessor.Value));
        }
    }

    public interface IInMemoryEventPublisherSettings : IConsumerSettings, IHaveMessageProcessor<IEvent>, ISettingsBuilder<IPublisher<IEvent>>
    {
    }

    public class InMemoryEventPublisherSettings : IInMemoryEventPublisherSettings
    {
        string IConsumerSettings.BoundedContext { get; set; }

        Lazy<IEndpointPostConsume> IHaveEndpointPostConsumeActions.PostConsume { get; set; }

        Serializer.ISerializer IHaveSerializer.Serializer { get; set; }

        Lazy<IMessageProcessor<IEvent>> IHaveMessageProcessor<IEvent>.MessageHandlerProcessor { get; set; }

        Lazy<IPublisher<IEvent>> ISettingsBuilder<IPublisher<IEvent>>.Build()
        {
            IInMemoryEventPublisherSettings settings = this as IInMemoryEventPublisherSettings;
            return new Lazy<IPublisher<IEvent>>(() => new InMemoryEventPublisher(settings.MessageHandlerProcessor.Value));
        }
    }

}
