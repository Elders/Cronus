using System;
using Elders.Cronus.DomainModelling;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Config
{
    public class ConsumerSettings<TContract> : IConsumerSettings<TContract> where TContract : IMessage
    {
        public ConsumerSettings()
        {
            this.WithNoEndpointPostConsume();
        }

        string IConsumerSettings.BoundedContext { get; set; }

        Lazy<IMessageProcessor<TContract>> IHaveMessageProcessor<TContract>.MessageHandlerProcessor { get; set; }

        Lazy<IEndpointPostConsume> IHaveEndpointPostConsumeActions.PostConsume { get; set; }

        ProtoregSerializer IHaveSerializer.Serializer { get; set; }

        Lazy<IConsumer<TContract>> ISettingsBuilder<IConsumer<TContract>>.Build()
        {
            IConsumerSettings<TContract> settings = this as IConsumerSettings<TContract>;
            return new Lazy<IConsumer<TContract>>(() => new EndpointConsumer<TContract>(settings.MessageHandlerProcessor.Value, settings.PostConsume.Value));
        }
    }
}