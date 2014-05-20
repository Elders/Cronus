using System;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Transport;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IHaveEndpointPostConsumeActions
    {
        Lazy<IEndpointPostConsume> PostConsume { get; set; }
    }

    public static class EndpointPostConsumeActionsExtensions
    {
        public static T WithDefaultEndpointPostConsume<T>(this T self, IHaveTransport<IPipelineTransport> transport) where T : IHaveEndpointPostConsumeActions, IHaveSerializer
        {
            self.PostConsume = new Lazy<IEndpointPostConsume>(() => new DefaultEndpointPostConsume(transport.Transport.Value.PipelineFactory, self.Serializer));
            return self;
        }

        public static T WithNoEndpointPostConsume<T>(this T self) where T : IHaveEndpointPostConsumeActions
        {
            self.PostConsume = new Lazy<IEndpointPostConsume>(() => new NoEndpointPostConsume());
            return self;
        }

        public static T SetConsumeSuccessStrategy<T>(this T self, IEndpointConsumerSuccessStrategy success) where T : IHaveEndpointPostConsumeActions
        {
            var newPostConsume = self.PostConsume;
            self.PostConsume = new Lazy<IEndpointPostConsume>(() =>
                {
                    newPostConsume.Value.SuccessStrategy = success;
                    return newPostConsume.Value;
                });
            return self;
        }
    }

    public interface IConsumerSettings : IHaveEndpointPostConsumeActions, IHaveSerializer
    {
        string BoundedContext { get; set; }
    }

    public interface IConsumerSettings<TContract> : IConsumerSettings, IHaveMessageProcessor<TContract>, ISettingsBuilder<IConsumer<TContract>> where TContract : IMessage
    {

    }
}
